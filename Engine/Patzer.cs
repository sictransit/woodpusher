using Serilog;
using SicTransit.Woodpusher.Common.Exceptions;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Lookup;
using SicTransit.Woodpusher.Common.Parsing;
using SicTransit.Woodpusher.Engine.Extensions;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System;
using System.Diagnostics;

namespace SicTransit.Woodpusher.Engine
{
    public class Patzer : IEngine
    {
        public IBoard Board { get; private set; }

        private readonly Random random = new();

        private CancellationTokenSource cancellationTokenSource = new();

        private readonly Stopwatch stopwatch = new();

        private const int MaxDepth = 16;

        public Patzer()
        {
            Board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);
        }

        public void Initialize()
        {
            Board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);
        }

        public void Play(Move move)
        {
            Log.Debug($"{Board.ActiveColor} plays: {move}");

            Board = Board.PlayMove(move);
        }

        public void Position(string fen, IEnumerable<AlgebraicMove>? algebraicMoves = null)
        {
            algebraicMoves ??= Enumerable.Empty<AlgebraicMove>();

            Board = ForsythEdwardsNotation.Parse(fen);

            foreach (var algebraicMove in algebraicMoves)
            {
                var move = Board.GetLegalMoves().SingleOrDefault(m => m.Piece.GetSquare().Equals(algebraicMove.From) && m.GetTarget().Equals(algebraicMove.To) && m.PromotionType == algebraicMove.Promotion);

                if (move == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(algebraicMoves), $"unable to play: {algebraicMove}");
                }

                Play(move);
            }
        }

        public AlgebraicMove FindBestMove(int timeLimit = 1000, Action<string>? infoCallback = null)
        {
            cancellationTokenSource = new CancellationTokenSource();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                Log.Information($"thinking time: {timeLimit}");
                Thread.Sleep(timeLimit);
                if (!cancellationTokenSource.IsCancellationRequested && !Debugger.IsAttached)
                {
                    cancellationTokenSource.Cancel();
                }
            });

            stopwatch.Restart();

            var openingMoves = Board.GetOpeningBookMoves();

            var nodes = (openingMoves.Any() ? openingMoves : Board.GetLegalMoves()).Select(m => new Node(m, MaxDepth)).ToList();

            if (!nodes.Any())
            {
                throw new PatzerException("No valid moves found for this board.");
            }

            Log.Information($"Legal moves for {Board.ActiveColor}: {string.Join(';', nodes.Select(n => n.Move))}");

            var cancellationToken = cancellationTokenSource.Token;

            var parallelOptions = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Debugger.IsAttached ? 1 : -1
            };

            var maxDepth = 0;

            while (!cancellationTokenSource.IsCancellationRequested && nodes.Count > 1 && maxDepth < MaxDepth )
            {
                if (nodes.Any(n => n.MateIn().HasValue && n.MateIn() > 0))
                {
                    break;
                }

                foreach (var chunk in nodes.OrderByDescending(e => e.AbsoluteScore).Chunk(Environment.ProcessorCount))
                {
                    try
                    {
                        Parallel.ForEach(chunk, parallelOptions, node =>
                        {
                            try
                            {
                                var score = EvaluateBoard(Board.PlayMove(node.Move), node, maxDepth, 0, -Scoring.MateScore * 4, Scoring.MateScore * 4, cancellationToken);

                                if (score.HasValue)
                                {
                                    node.Score = score.Value;

                                    if (infoCallback != null)
                                    {
                                        SendAnalysisInfo(infoCallback, maxDepth + 1, nodes.Sum(n => n.Count), node, stopwatch.ElapsedMilliseconds);
                                    }
                                }
                                else
                                {
                                    Log.Debug($"Discarding evaluation due to timeout: {node.Move} @ depth {maxDepth}");
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                if (infoCallback != null)
                                {
                                    SendExceptionInfo(infoCallback, ex);
                                }

                                throw;
                            }
                        });
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                maxDepth++;
            }

            var bestNodeGroup = nodes.GroupBy(e => e.AbsoluteScore).OrderByDescending(g => g.Key).First().ToArray();

            var bestNode = bestNodeGroup[random.Next(bestNodeGroup.Length)];

            Log.Debug($"evaluated {nodes.Sum(n => n.Count)} nodes, found: {bestNode}");

            return new AlgebraicMove(bestNode.Move);
        }

        private static void SendInfo(Action<string> callback, string info)
        {
            callback.Invoke($"info {info}");
        }


        private static void SendExceptionInfo(Action<string> callback, Exception exception)
        {
            SendInfo(callback, $"string exception {exception.GetType().Name} {exception.Message}");
        }

        private static void SendAnalysisInfo(Action<string> callback, int depth, long nodes, Node node, long time)
        {
            var preview = string.Join(" ", node.GetLine().Select(m => m.ToAlgebraicMoveNotation()));

            var mateIn = node.MateIn();
            var score = mateIn.HasValue ? $"mate {mateIn}" : $"cp {node.AbsoluteScore}";

            var nodesPerSecond = time == 0 ? 0 : nodes * 1000 / time;

            SendInfo(callback, $"depth {depth} nodes {nodes} score {score} time {time} pv {preview} nps {nodesPerSecond}");
        }

        private int? EvaluateBoard(IBoard board, Node node, int maxDepth, int depth, int alpha, int beta, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            var moves = board.GetLegalMoves();

            var maximizing = board.ActiveColor.Is(Piece.White);

            // ReSharper disable once PossibleMultipleEnumeration
            if (!moves.Any())
            {
                var mateScore = maximizing ? -Scoring.MateScore + depth + 1 : Scoring.MateScore - (depth + 1);
                return board.IsChecked ? mateScore : Scoring.DrawScore;
            }

            if (depth == maxDepth)
            {
                return board.Score;
            }

            var bestScore = maximizing ? -Scoring.MateScore * 2 : Scoring.MateScore * 2;

            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var move in moves)
            {
                node.Count++;

                var score = EvaluateBoard(board.PlayMove(move), node, maxDepth, depth + 1, alpha, beta, cancellationToken);

                if (!score.HasValue)
                {
                    break;
                }

                if (maximizing)
                {
                    if (score > bestScore)
                    {
                        node.Line[depth + 1] = move;
                        bestScore = score.Value;
                    }

                    if (bestScore >= beta)
                    {
                        break;
                    }

                    alpha = Math.Max(alpha, bestScore);
                }
                else
                {
                    if (score < bestScore)
                    {
                        node.Line[depth + 1] = move;
                        bestScore = score.Value;
                    }

                    if (bestScore <= alpha)
                    {
                        break;
                    }

                    beta = Math.Min(beta, bestScore);
                }
            }

            return bestScore;
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }
    }
}