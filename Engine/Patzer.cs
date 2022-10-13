using Serilog;
using SicTransit.Woodpusher.Common.Exceptions;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Lookup;
using SicTransit.Woodpusher.Common.Parsing;
using SicTransit.Woodpusher.Engine.Extensions;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Diagnostics;

namespace SicTransit.Woodpusher.Engine
{
    public class Patzer : IEngine
    {
        public IBoard Board { get; private set; }

        private readonly Random random = new();

        private CancellationTokenSource cancellationTokenSource = new();

        private readonly Stopwatch stopwatch = new();

        private long nodeCount;

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

            nodeCount = 0;
            stopwatch.Restart();

            var openingMoves = Board.GetOpeningBookMoves();

            var nodes = (openingMoves.Any() ? openingMoves : Board.GetLegalMoves()).Select(m => new Node(m)).ToList();

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

            var maxDepth = 1;

            while (!cancellationTokenSource.IsCancellationRequested && maxDepth <= MaxDepth)
            {
                if (nodes.Count <= 1)
                {
                    break;
                }

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
                            var score = EvaluateBoard(Board, node, maxDepth, 1, -Scoring.MateScore * 4, Scoring.MateScore * 4, cancellationToken);

                            if (!cancellationToken.IsCancellationRequested)
                            {
                                node.Score = score;

                                if (infoCallback != null)
                                {
                                    SendInfo(infoCallback, maxDepth, nodeCount, node, stopwatch.ElapsedMilliseconds, new[] { node.Move });
                                }
                            }
                        });
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                maxDepth += 2;
            }

            var bestNodeGroup = nodes.GroupBy(e => e.AbsoluteScore).OrderByDescending(g => g.Key).First().ToArray();

            var bestNode = bestNodeGroup[random.Next(bestNodeGroup.Length)];

            Log.Debug($"evaluated {nodeCount} nodes, found: {bestNode}");

            return new AlgebraicMove(bestNode.Move);
        }

        private static void SendInfo(Action<string> callback, int depth, long nodes, Node node, long time, IEnumerable<Move> line)
        {
            var preview = string.Join(" ", line.Select(m => m.ToAlgebraicMoveNotation()));

            var mateIn = node.MateIn();
            var score = mateIn.HasValue ? $"mate {mateIn}" : $"cp {node.AbsoluteScore}";

            var nodesPerSecond = time == 0 ? 0 : nodes * 1000 / time;

            callback.Invoke($"info depth {depth} nodes {nodes} score {score} time {time} pv {preview} nps {nodesPerSecond}");
        }

        private int EvaluateBoard(IBoard board, Node node, int maxDepth, int depth, int alpha, int beta, CancellationToken cancellationToken)
        {
            board = board.PlayMove(node.Move);

            var moves = board.GetLegalMoves();

            var maximizing = board.ActiveColor.Is(Piece.White);

            if (!moves.Any())
            {
                var mateScore = maximizing ? -Scoring.MateScore + depth + 1 : Scoring.MateScore - (depth + 1);
                return board.IsChecked ? mateScore : Scoring.DrawScore;
            }

            if (depth == maxDepth || cancellationToken.IsCancellationRequested)
            {
                return board.Score;
            }

            var bestScore = maximizing ? -Scoring.MateScore * 2 : Scoring.MateScore * 2;

            foreach (var move in moves)
            {
                nodeCount++;

                var score = EvaluateBoard(board, new Node(move), maxDepth, depth + 1, alpha, beta, cancellationToken);

                if (maximizing)
                {
                    bestScore = Math.Max(bestScore, score);

                    if (bestScore >= beta)
                    {
                        break;
                    }

                    alpha = Math.Max(alpha, bestScore);
                }
                else
                {
                    bestScore = Math.Min(bestScore, score);

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