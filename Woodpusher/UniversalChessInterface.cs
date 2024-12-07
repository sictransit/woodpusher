using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Common.Extensions;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Parsing;
using SicTransit.Woodpusher.Engine;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher
{
    public class UniversalChessInterface
    {
        private readonly Action<string, bool> consoleOutput;
        private readonly CancellationToken quitToken;

        private static readonly Regex UciCommand = new(@"^uci$", RegexOptions.Compiled);
        private static readonly Regex IsReadyCommand = new(@"^isready$", RegexOptions.Compiled);
        private static readonly Regex UciNewGameCommand = new(@"^ucinewgame$", RegexOptions.Compiled);
        private static readonly Regex PositionCommand = new(@"^position", RegexOptions.Compiled);
        private static readonly Regex GoCommand = new(@"^go", RegexOptions.Compiled);
        private static readonly Regex DisplayCommand = new(@"^d$", RegexOptions.Compiled);
        private static readonly Regex SetOptionCommand = new(@"^setoption", RegexOptions.Compiled);

        private static readonly Regex PositionRegex =
            new(@"^(position).+?(fen(.+?))?(moves(.+?))?$", RegexOptions.Compiled);
        private static readonly Regex MovesRegex = new(@"([a-h][1-8][a-h][1-8][rnbq]?)", RegexOptions.Compiled);
        private static readonly Regex WhiteTimeRegex = new(@"wtime (-?\d+)", RegexOptions.Compiled);
        private static readonly Regex BlackTimeRegex = new(@"btime (-?\d+)", RegexOptions.Compiled);
        private static readonly Regex MovesToGoRegex = new(@"movestogo (\d+)", RegexOptions.Compiled);
        private static readonly Regex MovetimeRegex = new(@"movetime (\d+)", RegexOptions.Compiled);
        private static readonly Regex PerftRegex = new(@"perft (\d+)", RegexOptions.Compiled);
        private static readonly Regex OptionRegex = new(@"^setoption name (\w+) value (\w+)$", RegexOptions.Compiled);

        private const int EngineLatency = 10;

        private readonly ManualResetEvent commandAvailable = new(false);
        private readonly ConcurrentQueue<string> commandQueue = new();

        private readonly EngineOptions engineOptions = new() { UseOpeningBook = true };

        private volatile IEngine engine;

        public bool Quit { get; private set; }

        public UniversalChessInterface(Action<string, bool> consoleOutput, CancellationToken quitToken)
        {
            this.consoleOutput = consoleOutput;
            this.quitToken = quitToken;
        }

        public void Stop()
        {
            engine?.Stop();
        }

        public void Run() => Task.Run(EngineLoop);

        private void EngineLoop()
        {
            engine = new Patzer(consoleOutput);
            engine.Initialize(engineOptions);
            while (!quitToken.IsCancellationRequested)
            {
                commandAvailable.WaitOne(20);

                while (commandQueue.TryDequeue(out var command))
                {
                    ProcessCommand(engine, command);
                }

                commandAvailable.Reset();
            }
        }

        public void EnqueueCommand(string command)
        {
            commandQueue.Enqueue(command);
            commandAvailable.Set();
        }

        private void ProcessCommand(IEngine engine, string command)
        {
            if (UciCommand.IsMatch(command))
            {
                Uci(engine);
            }
            else if (UciNewGameCommand.IsMatch(command))
            {
                Initialize(engine);
            }
            else if (IsReadyCommand.IsMatch(command))
            {
                IsReady();
            }
            else if (PositionCommand.IsMatch(command))
            {
                Position(engine, command);
            }
            else if (DisplayCommand.IsMatch(command))
            {
                Display(engine);
            }
            else if (SetOptionCommand.IsMatch(command))
            {
                SetOption(command);
            }
            else if (GoCommand.IsMatch(command))
            {
                Go(engine, command);
            }
            else
            {
                Log.Warning("Ignored unknown command: {Command}", command);
            }
        }

        private void Output(string message, bool isInfo = false)
        {
            consoleOutput(message, isInfo);
        }

        private void Uci(IEngine engine)
        {
            var version = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            Output($"id name Woodpusher {version}");
            Output("id author Mikael Fredriksson <micke@sictransit.net>");
            Output("option name OwnBook type check default true");
            Output("uciok");
        }

        private void Display(IEngine engine) => Output(engine.Board.PrettyPrint());

        private void Initialize(IEngine engine) => engine.Initialize(engineOptions);

        private void IsReady() => Output("readyok");

        private void SetOption(string command)
        {
            var match = OptionRegex.Match(command);
            if (!match.Success)
            {
                Log.Error("Unable to parse: {Command}", command);
                return;
            }
            var name = match.Groups[1].Value;
            var value = match.Groups[2].Value;
            if (name == "OwnBook")
            {
                engineOptions.UseOpeningBook = bool.Parse(value);
            }
        }

        private void Position(IEngine engine, string command)
        {
            var match = PositionRegex.Match(command);

            if (!match.Success)
            {
                Log.Error("Unable to parse: {Command}", command);

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
                    Log.Information("Failed to parse piece: {Command}", command);
                }
            }

            engine.Position(fen, moves);
        }

        private void Go(IEngine engine, string command)
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
                        Output($"info string movestogo not specified, using {movesToGo}", true);
                    }

                    timeLimit = Math.Min(timeLimit, timeLeft / movesToGo);
                }

                try
                {
                    var bestMove = engine.FindBestMove(Math.Max(0, timeLimit - EngineLatency));

                    Output($"bestmove {(bestMove == null ? "(none)" : bestMove.Notation)}");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to find best move.");
                }
            }
        }
    }
}