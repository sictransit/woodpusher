using Serilog;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
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
                lock (engine)
                {
                    engine.Initialize();
                }
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
            consoleOutput($"id name Woodpusher v0.0");
            consoleOutput($"id author Mikael Fredriksson <micke@sictransit.net>");
            consoleOutput("uciok");
        }

        private void IsReady(object? o)
        {
            consoleOutput("readyok");
        }

        private void Position(object? o)
        {
            lock (engine)
            {
                try
                {
                    var parts = o.ToString().Split();

                    if (AlgebraicMove.TryParse(parts.Last(), out var algebraicMove))
                    {
                        Log.Information($"telling engine to play: {algebraicMove}");

                        engine.Play(algebraicMove!);
                    }
                    else
                    {
                        Log.Information($"failed to parse position: {o}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Caught exception in Position().", ex);
                    Log.Error(ex.ToString());
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
                    var move = engine.PlayBestMove();

                    consoleOutput($"bestmove {move.Notation}");
                }
                catch (Exception ex)
                {
                    Log.Error("Caught exception in Go().", ex);
                    throw;
                }
            }
        }
    }
}
