using Serilog;
using SicTransit.Woodpusher.Common.Exceptions;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Parsing;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
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

        private readonly IDictionary<string, int> repetitions = new Dictionary<string, int>();

        private readonly IDictionary<ulong, PrincipalVariationNode> hashTable = new Dictionary<ulong, PrincipalVariationNode>();

        public Patzer()
        {
            Board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);
        }

        public void Initialize()
        {
            Board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);
            repetitions.Clear();
            hashTable.Clear();
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

            while (!cancellationTokenSource.IsCancellationRequested && nodes.Count > 1)
            {
                if (nodes.Any(n => n.MateIn > 0))
                {
                    break;
                }

                var tasks = nodes.Where(n => !n.MateIn.HasValue).Select(node => Task.Run(() =>
                {
                    try
                    {
                        var score = EvaluateBoard(Board.PlayMove(node.Move), node, 1, -Declarations.BoardMaximumScore, Declarations.BoardMaximumScore, cancellationToken);

                        if (!cancellationToken.IsCancellationRequested)
                        {
                            node.Score = score;                            

                            if (infoCallback != null)
                            {
                                SendAnalysisInfo(infoCallback, node.MaxDepth, nodes.Sum(n => n.Count), node, stopwatch.ElapsedMilliseconds);
                            }
                        }
                        else
                        {
                            if (infoCallback != null)
                            {
                                SendDebugInfo(infoCallback, $"aborting {node.Move.ToAlgebraicMoveNotation()} @ depth {node.MaxDepth}");
                            }

                            Log.Debug($"Discarding evaluation due to timeout: {node.Move} @ depth {node.MaxDepth}");
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

                })).ToArray();

                Task.WaitAll(tasks);

                nodes.ForEach(n => n.MaxDepth += 2);
            }


            // Set node score to zero for threefold repetition moves.
            UpdateForThreefoldRepetition(nodes);

            var bestNodeGroup = nodes.GroupBy(e => e.AbsoluteScore).OrderByDescending(g => g.Key).First().ToArray();

            var bestNode = bestNodeGroup[random.Next(bestNodeGroup.Length)];

            Log.Debug($"evaluated {nodes.Sum(n => n.Count)} nodes, found: {bestNode}");

            return new AlgebraicMove(bestNode.Move);
        }        

        private void UpdateForThreefoldRepetition(List<Node> nodes)
        {
            foreach (var node in nodes)
            {
                var algebraic = node.Move.ToAlgebraicMoveNotation();
                var key = $"{Board.Hash}_{algebraic}";

                if (repetitions.ContainsKey(key))
                {
                    repetitions[key] += 1;
                }
                else
                {
                    repetitions.Add(key, 1);
                }

                if (repetitions[key] > 1)
                {
                    Log.Information($"Resetting score to zero for threefold repetion of: {algebraic}");

                    node.Score = 0;
                }
            }
        }

        private static void SendInfo(Action<string> callback, string info)
        {
            callback.Invoke($"info {info}");
        }

        private static void SendDebugInfo(Action<string> callback, string info)
        {
            SendInfo(callback, $"string debug {info}");
        }

        private static void SendExceptionInfo(Action<string> callback, Exception exception)
        {
            SendInfo(callback, $"string exception {exception.GetType().Name} {exception.Message}");
        }

        private static void SendAnalysisInfo(Action<string> callback, int depth, long nodes, Node node, long time)
        {
            var principalVariation = node.Move.ToAlgebraicMoveNotation();

            var score = node.MateIn.HasValue ? $"mate {node.MateIn.Value}" : $"cp {node.AbsoluteScore}";

            var nodesPerSecond = time == 0 ? 0 : nodes * 1000 / time;

            SendInfo(callback, $"depth {depth} nodes {nodes} score {score} time {time} pv {principalVariation} nps {nodesPerSecond}");
        }

        private int EvaluateBoard(IBoard board, Node node, int depth, int alpha, int beta, CancellationToken cancellationToken)
        {
            var maximizing = board.ActiveColor.Is(Piece.White);
            var bestScore = maximizing ? -Declarations.MoveMaximumScore : Declarations.MoveMaximumScore;

            if (cancellationToken.IsCancellationRequested)
            {
                return bestScore;
            }

            var moves = board.GetLegalMoves();

            // ReSharper disable once PossibleMultipleEnumeration
            if (!moves.Any())
            {
                var mateScore = maximizing ? -Declarations.MateScore + depth + 1 : Declarations.MateScore - (depth + 1);
                return board.IsChecked ? mateScore : Declarations.DrawScore;
            }

            if (depth == node.MaxDepth)
            {
                return board.Score;
            }            

            if (hashTable.TryGetValue(board.Hash, out var pvNode))
            {
                moves = moves.OrderByDescending(m => m.Equals(pvNode.Move));
            }

            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var move in moves)
            {
                node.Count++;

                var score = EvaluateBoard(board.PlayMove(move), node, depth + 1, alpha, beta, cancellationToken);

                if (maximizing)
                {
                    bestScore = Math.Max(score, bestScore);

                    if (bestScore >= beta)
                    {
                        UpdateHashTable(board.Hash, move, score);
                        break;
                    }

                    alpha = Math.Max(alpha, bestScore);
                }
                else
                {
                    bestScore = Math.Min(score, bestScore);

                    if (bestScore <= alpha)
                    {
                        UpdateHashTable(board.Hash, move, -score);
                        break;
                    }

                    beta = Math.Min(beta, bestScore);
                }
            }

            return bestScore;
        }

        private void UpdateHashTable(ulong hash, Move move, int score)
        {
            lock (hashTable)
            {
                if (hashTable.TryGetValue(hash, out var pvNode))
                {
                    if (score > pvNode.Score)
                    {
                        pvNode.Move = move;
                        pvNode.Score = score;   
                    }
                }
                else
                {
                    hashTable.Add(hash, new PrincipalVariationNode(move, score));
                }
            }
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }
    }
}