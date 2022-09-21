using System.Reflection;
using Serilog;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Parsing;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher
{
    public class UniversalChessInterface
    {
        private volatile Action<string> consoleOutput;

        private static readonly Regex UciCommand = new(@"^uci$", RegexOptions.Compiled);
        private static readonly Regex IsReadyCommand = new(@"^isready$", RegexOptions.Compiled);
        private static readonly Regex UciNewGameCommand = new(@"^ucinewgame$", RegexOptions.Compiled);
        private static readonly Regex QuitCommand = new(@"^quit$", RegexOptions.Compiled);
        private static readonly Regex PositionCommand = new(@"^position", RegexOptions.Compiled);
        private static readonly Regex GoCommand = new(@"^go", RegexOptions.Compiled);

        private static readonly Regex PositionRegex =
            new(@"^(position).+?(fen(.+?))?(moves(.+?))?$", RegexOptions.Compiled);
        private static readonly Regex MovesRegex = new(@"([a-h][1-8][a-h][1-8][rnbq]?)", RegexOptions.Compiled);

        private volatile IEngine engine;

        public UniversalChessInterface(Action<string> consoleOutput, IEngine engine)
        {
            this.consoleOutput = consoleOutput;
            this.engine = engine;
        }

        public void ProcessCommand(string command)
        {
            Log.Debug($"Processing command: {command}");

            if (UciCommand.IsMatch(command))
            {
                ThreadPool.QueueUserWorkItem(Uci);
            }
            else if (UciNewGameCommand.IsMatch(command))
            {
                ThreadPool.QueueUserWorkItem(Initialize);
            }
            else if (IsReadyCommand.IsMatch(command))
            {
                ThreadPool.QueueUserWorkItem(IsReady);
            }
            else if (PositionCommand.IsMatch(command))
            {
                ThreadPool.QueueUserWorkItem(Position, command);
            }
            else if (GoCommand.IsMatch(command))
            {
                ThreadPool.QueueUserWorkItem(Go, command);
            }
            else if (QuitCommand.IsMatch(command))
            {
                Quit = true;
            }
            else
            {
                Log.Warning($"Ignored unknown command: {command}");
            }
        }

        public bool Quit { get; private set; }

        private void Uci(object? o)
        {
            lock (engine)
            {
                consoleOutput("id name Woodpusher v0.1.1");
                consoleOutput("id author Mikael Fredriksson <micke@sictransit.net>");
                consoleOutput("uciok");
            }
        }

        private void Initialize(object? o)
        {
            lock (engine)
            {
                engine.Initialize();
            }
        }

        private void IsReady(object? o)
        {
            lock (engine)
            {
                consoleOutput("readyok");
            }
        }

        private void Position(object? o)
        {
            lock (engine)
            {
                try
                {
                    var match = PositionRegex.Match(o.ToString());

                    if (!match.Success)
                    {
                        Log.Error($"Unable to parse: {o}");

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
                            Log.Information($"failed to parse position: {o}");
                        }
                    }

                    engine.Position(fen, moves);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Caught exception in Position().");
                    throw;
                }
            }
        }

        private void Go(object? o)
        {
            lock (engine)
            {
                try
                {
                    var move = engine.FindBestMove();

                    consoleOutput($"bestmove {move.Notation}");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Caught exception in Go().");
                    throw;
                }
            }
        }
    }
}
