using Serilog;
using SicTransit.Woodpusher.Common.Exceptions;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Lookup;
using SicTransit.Woodpusher.Common.Parsing;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Diagnostics;
using System.Xml.Linq;

namespace SicTransit.Woodpusher.Engine
{
    public class Patzer : IEngine
    {
        public IBoard Board { get; private set; }

        private readonly Random random = new();

        private CancellationTokenSource cancellationTokenSource = new();

        private readonly Stopwatch stopwatch = new();

        private volatile int nodeCount;

        public const int MATE_SCORE = 1000000;
        public const int DRAW_SCORE = 0;

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
                Log.Information($"thinking time: {timeLimit}");
                Thread.Sleep(timeLimit);
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();
                }
            });

            nodeCount = 0;
            stopwatch.Restart();

            var sign = Board.ActiveColor == PieceColor.White ? 1 : -1;

            var nodes = Board.GetLegalMoves().Select(m => new Node(m)).ToList();

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

            while (!cancellationTokenSource.IsCancellationRequested && maxDepth <= MAX_DEPTH)
            {
                if (nodes.Count <= 1)
                {
                    break;
                }

                //if (nodes.Any(e => Math.Abs(e.AbsoluteScore - Scoring.MATE_SCORE) <= MAX_DEPTH))
                //{
                //    break;
                //}

                foreach (var chunk in nodes.OrderByDescending(e => e.AbsoluteScore).Chunk(Environment.ProcessorCount))
                {
                    try
                    {
                        Parallel.ForEach(chunk, parallelOptions, node =>
                        {
                            var score = EvaluateBoard(Board.PlayMove(node.Move), maxDepth, 1, -MATE_SCORE, MATE_SCORE, cancellationToken) ;

                            if (!cancellationToken.IsCancellationRequested)
                            {
                                node.UpdateScoreIfBetter(score);

                                if (infoCallback != null)
                                {
                                    SendInfo(infoCallback, maxDepth, nodeCount, node, sign, stopwatch.ElapsedMilliseconds, new[] { node.Move });
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

        private static void SendInfo(Action<string> callback, int depth, int nodes, Node node, int perspective, long time, IEnumerable<Move> line)
        {
            var preview = string.Join(" ", line.Select(m => m.ToAlgebraicMoveNotation()));
            var nodesPerSecond = nodes * 1000 / (1 + time);

            string score;
            var mateIn = Math.Abs(node.AbsoluteScore- MATE_SCORE);
            if (mateIn <= depth)
            {
                var mateSign = Math.Sign(perspective * node.Score);
                score = $"mate {mateSign * mateIn / 2}";
            }
            else
            {
                score = $"cp {node.AbsoluteScore}";
            }

            callback.Invoke($"info depth {depth} nodes {nodes} score {score} time {time} pv {preview} nps {nodesPerSecond}");
        }

        private int EvaluateBoard(IBoard board, int maxDepth, int depth, int alpha, int beta, CancellationToken cancellationToken)
        {
            var legalMoves = board.GetLegalMoves();

            var maximizing = board.ActiveColor == PieceColor.White;

            if (!legalMoves.Any())
            {
                return board.IsChecked ? maximizing ? -MATE_SCORE + depth : MATE_SCORE - depth : DRAW_SCORE;
            }

            if (depth == maxDepth || cancellationToken.IsCancellationRequested)
            {
                return board.Score;
            }

            var bestScore = maximizing ? int.MinValue : int.MaxValue;

            foreach (var move in legalMoves)
            {
                nodeCount++;

                var score = EvaluateBoard(board.PlayMove(move), maxDepth, depth +1, alpha, beta, cancellationToken);

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