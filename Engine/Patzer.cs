using Serilog;
using SicTransit.Woodpusher.Common.Exceptions;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Interfaces;
using SicTransit.Woodpusher.Parsing;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SicTransit.Woodpusher.Engine
{
    public class Patzer : IEngine
    {
        public IBoard Board { get; private set; }

        private readonly Random random = new();

        private CancellationTokenSource cancellationTokenSource = new();

        private readonly Stopwatch stopwatch = new();
        private volatile int nodeCount;
        private int lastBoardHash;

        private readonly ConcurrentDictionary<int, int> hashTable = new();

        private const int MATE_SCORE = 1000000;
        private const int WHITE_MATE_SCORE = -MATE_SCORE;
        private const int BLACK_MATE_SCORE = MATE_SCORE;
        private const int DRAW_SCORE = 0;
        private const int MAX_DEPTH = 32;

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
            Log.Information($"{Board.ActiveColor} plays: {move}");

            Board = Board.PlayMove(move);
        }

        public void Position(string fen, IEnumerable<AlgebraicMove>? algebraicMoves = null)
        {
            algebraicMoves ??= Enumerable.Empty<AlgebraicMove>();

            Board = ForsythEdwardsNotation.Parse(fen);

            foreach (var algebraicMove in algebraicMoves)
            {
                var move = Board.GetLegalMoves().SingleOrDefault(m => m.Position.Square.Equals(algebraicMove.From) && m.GetTarget().Equals(algebraicMove.To) && m.PromotionType == algebraicMove.Promotion);

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
                Thread.Sleep(timeLimit);
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();
                }
            });

            hashTable.Clear();
            nodeCount = 0;
            stopwatch.Restart();

            var sign = Board.ActiveColor == PieceColor.White ? 1 : -1;

            var nodes = Board.GetLegalMoves().Select(m => new Node(m, 0)).ToList();

            if (!nodes.Any())
            {
                throw new PatzerException("No valid moves found for this board.");
            }

            Log.Information($"Legal moves for {Board.ActiveColor}: {string.Join(';', nodes.Select(n => n.Move))}");

            var cancellationToken = cancellationTokenSource.Token;

            var parallelOptions = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = -1
            };

            var depth = 1;

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                if (nodes.Count <= 1)
                {
                    break;
                }

                if (nodes.Any(e => Math.Abs(e.Score - MATE_SCORE) < MAX_DEPTH))
                {
                    break;
                }

                foreach (var chunk in nodes.OrderByDescending(e => e.Score).Chunk(Environment.ProcessorCount))
                {
                    try
                    {
                        Parallel.ForEach(chunk, parallelOptions, node =>
                        {
                            var board = Board.PlayMove(node.Move);
                            node.Hash = board.Hash;

                            var score = EvaluateBoard(board, depth, node, int.MinValue, int.MaxValue, cancellationToken);

                            if (!cancellationToken.IsCancellationRequested)
                            {
                                if (score > node.Score)
                                {
                                    node.Score = score;

                                    infoCallback?.Invoke($"info depth {depth} nodes {nodeCount} score cp {node.Score} time {stopwatch.ElapsedMilliseconds} pv {node.GetLine()} nps {nodeCount * 1000 / (1 + stopwatch.ElapsedMilliseconds)}");
                                }
                            }
                        });
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                depth += 2;
            }

            var bestNodeGroup = nodes.Where(n => n.Hash != lastBoardHash).GroupBy(e => e.Score).OrderByDescending(g => g.Key * sign).First().ToArray();

            var bestNode = bestNodeGroup[random.Next(bestNodeGroup.Length)];

            lastBoardHash = bestNode.Hash;

            Log.Information($"evaluated {nodeCount} nodes, found: {bestNode}");

            return new AlgebraicMove(bestNode.Move);
        }

        private int EvaluateBoard(IBoard board, int depth, Node parentNode, int alpha, int beta, CancellationToken cancellationToken)
        {
            if (!parentNode.Nodes.Any())
            {
                parentNode.Nodes.AddRange(board.GetLegalMoves().Select(m => new Node(m, parentNode.Level + 1)));
            }

            var maximizing = board.ActiveColor == PieceColor.White;

            if (!parentNode.Nodes.Any())
            {
                return board.IsChecked ? maximizing ? WHITE_MATE_SCORE - depth : BLACK_MATE_SCORE + depth : DRAW_SCORE;
            }

            if (depth == 0 || cancellationToken.IsCancellationRequested)
            {
                return board.Score;
            }

            var bestScore = maximizing ? int.MinValue : int.MaxValue;
            var sign = maximizing ? 1 : -1;

            foreach (var node in parentNode.Nodes.OrderByDescending(n => n.Score * sign))
            {
                nodeCount++;

                var newBoard = board.PlayMove(node.Move);

                node.Score = EvaluateBoard(newBoard, depth - 1, node, alpha, beta, cancellationToken);
                node.Hash = newBoard.Hash;

                if (maximizing)
                {
                    bestScore = Math.Max(bestScore, node.Score);

                    if (bestScore >= beta)
                    {
                        break;
                    }

                    alpha = Math.Max(alpha, bestScore);
                }
                else
                {
                    bestScore = Math.Min(bestScore, node.Score);

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