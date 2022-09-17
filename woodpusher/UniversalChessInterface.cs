using Serilog;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Engine;
using SicTransit.Woodpusher.Model.Extensions;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher
{
    public class UniversalChessInterface
    {
        private volatile Action<string> consoleOutput;

        private static Regex uciCommand = new(@"^uci$", RegexOptions.Compiled);
        private static Regex isReadyCommand = new(@"^isready$", RegexOptions.Compiled);
        private static Regex uciNewGameCommand = new(@"^ucinewgame$", RegexOptions.Compiled);
        private static Regex quitCommand = new(@"^quit$", RegexOptions.Compiled);
        private static Regex positionCommand = new(@"^position", RegexOptions.Compiled);
        private static Regex goCommand = new(@"^go", RegexOptions.Compiled);

        private volatile IEngine engine;

        public void RegisterConsoleCallback(Action<string> consoleOutput)
        {
            this.consoleOutput = consoleOutput;
        }

        public void ProcessCommand(string command)
        {
            Log.Debug($"Processing command: {command}");

            if (uciCommand.IsMatch(command))
            {
                ThreadPool.QueueUserWorkItem(Uci);
            }
            else if (uciNewGameCommand.IsMatch(command))
            {
                engine = new Patzer();
            }
            else if (isReadyCommand.IsMatch(command))
            {
                ThreadPool.QueueUserWorkItem(IsReady);
            }
            else if (positionCommand.IsMatch(command))
            {
                ThreadPool.QueueUserWorkItem(Position, command);
            }
            else if (goCommand.IsMatch(command))
            {
                ThreadPool.QueueUserWorkItem(Go, command);
            }
            else if (quitCommand.IsMatch(command))
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
            var parts = o.ToString().Split();

            var move = parts.Last();

            if (move.IsAlgebraicMoveNotation())
            {
                lock (engine)
                {
                    engine.Play(move);
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

                    consoleOutput($"bestmove {move.ToAlgebraicMoveNotation()}");
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
