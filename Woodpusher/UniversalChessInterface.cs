using Serilog;
using SicTransit.Woodpusher.Common.Extensions;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Parsing;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher
{
    public class UniversalChessInterface
    {
        private readonly Action<string> consoleOutput;

        private static readonly Regex UciCommand = new(@"^uci$", RegexOptions.Compiled);
        private static readonly Regex IsReadyCommand = new(@"^isready$", RegexOptions.Compiled);
        private static readonly Regex UciNewGameCommand = new(@"^ucinewgame$", RegexOptions.Compiled);
        private static readonly Regex QuitCommand = new(@"^quit$", RegexOptions.Compiled);
        private static readonly Regex StopCommand = new(@"^stop$", RegexOptions.Compiled);
        private static readonly Regex PositionCommand = new(@"^position", RegexOptions.Compiled);
        private static readonly Regex GoCommand = new(@"^go", RegexOptions.Compiled);
        private static readonly Regex DisplayCommand = new(@"^d$", RegexOptions.Compiled);


        private static readonly Regex PositionRegex =
            new(@"^(position).+?(fen(.+?))?(moves(.+?))?$", RegexOptions.Compiled);
        private static readonly Regex MovesRegex = new(@"([a-h][1-8][a-h][1-8][rnbq]?)", RegexOptions.Compiled);
        private static readonly Regex WhiteTimeRegex = new(@"wtime (\d+)", RegexOptions.Compiled);
        private static readonly Regex BlackTimeRegex = new(@"btime (\d+)", RegexOptions.Compiled);
        private static readonly Regex MovesToGoRegex = new(@"movestogo (\d+)", RegexOptions.Compiled);
        private static readonly Regex MovetimeRegex = new(@"movetime (\d+)", RegexOptions.Compiled);
        private static readonly Regex PerftRegex = new(@"perft (\d+)", RegexOptions.Compiled);

        private const int engineLatency = 100;

        private readonly IEngine engine;

        public UniversalChessInterface(Action<string> consoleOutput, IEngine engine)
        {
            this.consoleOutput = consoleOutput;
            this.engine = engine;
        }

        public void ProcessCommand(string command)
        {
            Log.Debug($"Processing command: {command}");

            Task? task = null;

            if (UciCommand.IsMatch(command))
            {
                task = Uci();
            }
            else if (UciNewGameCommand.IsMatch(command))
            {
                task = Initialize();
            }
            else if (IsReadyCommand.IsMatch(command))
            {
                task = IsReady();
            }
            else if (PositionCommand.IsMatch(command))
            {
                task = Position(command);
            }
            else if (GoCommand.IsMatch(command))
            {
                task = Go(command);
            }
            else if (StopCommand.IsMatch(command))
            {
                task = Stop();
            }
            else if (DisplayCommand.IsMatch(command))
            {
                task = Display();
            }
            else if (QuitCommand.IsMatch(command))
            {
                Quit = true;
            }
            else
            {
                Log.Warning($"Ignored unknown command: {command}");
            }

            task?.ContinueWith(t =>
            {
                if (t.IsFaulted && t.Exception != null)
                {
                    Log.Error(t.Exception, "Engine task threw an Exception.");
                }
            });
        }

        public bool Quit { get; private set; }

        private Task Uci()
        {
            return Task.Run(() =>
            {
                lock (engine)
                {
                    var version = Assembly.GetExecutingAssembly()
                      .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                      .InformationalVersion;

                    consoleOutput($"id name Woodpusher {version}");
                    consoleOutput("id author Mikael Fredriksson <micke@sictransit.net>");
                    //consoleOutput("option name Ponder type check default false");
                    consoleOutput("uciok");
                }
            });
        }

        private Task Initialize()
        {
            return Task.Run(() =>
            {
                lock (engine)
                {
                    engine.Initialize();
                }
            });
        }

        private Task IsReady()
        {
            return Task.Run(() =>
            {
                lock (engine)
                {
                    consoleOutput("readyok");
                }
            });
        }

        private Task Stop()
        {
            return Task.Run(() =>
            {
                engine.Stop();
            });
        }

        private Task Position(string command)
        {
            return Task.Run(() =>
            {
                lock (engine)
                {
                    var match = PositionRegex.Match(command);

                    if (!match.Success)
                    {
                        Log.Error($"Unable to parse: {command}");

                        return;
                    }

                    var fen = match.Groups[3].Value;

                    if (string.IsNullOrWhiteSpace(fen))
                    {
                        fen = ForsythEdwardsNotation.StartingPosition;
                    }

                    var moves = new List<AlgebraicMove>();

                    if (!string.IsNullOrWhiteSpace(match.Groups[5].Value))
                    {
                        var matches = MovesRegex.Matches(match.Groups[5].Value);

                        if (matches.Count > 0)
                        {
                            moves.AddRange(matches.Select(m => AlgebraicMove.Parse(m.Value)));
                        }
                        else
                        {
                            Log.Information($"failed to parse piece: {command}");
                        }
                    }

                    engine.Position(fen, moves);
                }
            });
        }

        private Task Go(string command)
        {
            return Task.Run(() =>
            {
                lock (engine)
                {
                    var movesToGoMatch = MovesToGoRegex.Match(command);
                    var whiteTimeMatch = WhiteTimeRegex.Match(command);
                    var blackTimeMatch = BlackTimeRegex.Match(command);
                    var movetimeMatch = MovetimeRegex.Match(command);
                    var perftMatch = PerftRegex.Match(command);

                    if (perftMatch.Success)
                    {
                        var depth = int.Parse(perftMatch.Groups[1].Value);

                        engine.Perft(depth);
                    }
                    else
                    {
                        var timeLimit = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;

                        if (movetimeMatch.Success)
                        {
                            timeLimit = int.Parse(movetimeMatch.Groups[1].Value);
                        }
                        else if (whiteTimeMatch.Success && blackTimeMatch.Success)
                        {
                            var timeLeft = int.Parse(engine.Board.ActiveColor.Is(Piece.White) ? whiteTimeMatch.Groups[1].Value : blackTimeMatch.Groups[1].Value);

                            int movesToGo;

                            if (movesToGoMatch.Success)
                            {
                                movesToGo = int.Parse(movesToGoMatch.Groups[1].Value);
                            }
                            else
                            {
                                movesToGo = Math.Max(8, 40 - (engine.Board.Counters.FullmoveNumber % 40));
                                consoleOutput($"info string movestogo not specified, using {movesToGo}");
                            }

                            timeLimit = Math.Min(timeLimit, timeLeft / movesToGo);
                        }

                        try
                        {
                            var bestMove = engine.FindBestMove(Math.Max(0, timeLimit - engineLatency));

                            consoleOutput($"bestmove {(bestMove == null ? "(none)" : bestMove.Notation)}");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Failed to find best move.");                            
                        }
                    }
                }
            });
        }

        private Task Display()
        {
            return Task.Run(() =>
            {
                lock (engine)
                {
                    consoleOutput(engine.Board.PrettyPrint());
                }
            });
        }
    }
}
