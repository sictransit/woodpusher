﻿using Serilog;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Interfaces;
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
        private static readonly Regex StopCommand = new(@"^stop$", RegexOptions.Compiled);
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
                task = Go();
            }
            else if (StopCommand.IsMatch(command))
            {
                task = Stop();
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
                    consoleOutput("id name Woodpusher v0.1.2");
                    consoleOutput("id author Mikael Fredriksson <micke@sictransit.net>");
                    consoleOutput("uciok");

                    throw new NotImplementedException();
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

        private Task Position(object? o)
        {
            return Task.Run(() =>
            {
                lock (engine)
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
            });
        }

        private Task Go()
        {
            return Task.Run(() =>
            {
                Action<string> infoCallback = new(s => consoleOutput(s));

                var move = engine.FindBestMove(5000, infoCallback);

                consoleOutput($"bestmove {move.Notation}");
            });
        }
    }
}
