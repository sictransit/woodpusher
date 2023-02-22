using Serilog;
using SicTransit.Woodpusher.Common.Exceptions;
using SicTransit.Woodpusher.Common.Extensions;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Lookup;
using SicTransit.Woodpusher.Common.Parsing;
using SicTransit.Woodpusher.Engine.Enums;
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

        private readonly ConcurrentDictionary<ulong, (Move, int)> hashTable = new();
        private readonly OpeningBook openingBook = new();
        private readonly Action<string>? infoCallback;

        public Patzer(Action<string>? infoCallback = null)
        {
            Board = Common.Board.StartingPosition();

            this.infoCallback = infoCallback;
        }

        public void Initialize()
        {
            Board = Common.Board.StartingPosition();
        }

        private void SendCallbackInfo(string info) => infoCallback?.Invoke(info);

        public void Play(Move move)
        {
            var color = Board.ActiveColor.Is(Piece.White) ? "White" : "Black";

            Log.Debug($"{color} plays: {move}");

            Board = Board.Play(move);
        }

        private Move? GetOpeningBookMove()
        {
            var openingBookMoves = openingBook.GetMoves(Board.Hash);

            var legalMoves = Board.GetLegalMoves().ToArray();

            var legalOpeningBookMoves = openingBookMoves.Select(o => new { openingBookMove = o, legalMove = legalMoves.SingleOrDefault(m => m.ToAlgebraicMoveNotation().Equals(o.Move.Notation)) }).Where(l => l.legalMove != null).ToArray();

            return legalOpeningBookMoves.OrderByDescending(m => random.Next(m.openingBookMove.Count)).ThenByDescending(_ => random.NextDouble()).FirstOrDefault()?.legalMove;
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

        public void Perft(int depth)
        {
            cancellationTokenSource = new CancellationTokenSource();

            var initialMoves = Board.GetLegalMoves().Select(m => new { move = m, board = Board.Play(m) });

            ulong total = 0;

            var options = new ParallelOptions
            {
                CancellationToken = cancellationTokenSource.Token
            };

            Parallel.ForEach(initialMoves, options, i =>
            {
                ulong nodes = 0;

                if (depth > 1)
                {
                    nodes += i.board.Perft(depth);
                }
                else
                {
                    nodes = 1;
                }

                Interlocked.Add(ref total, nodes);

                SendCallbackInfo($"{i.move.ToAlgebraicMoveNotation()}: {nodes}");
            });

            SendCallbackInfo(Environment.NewLine + $"Nodes searched: {total}");
        }

        public BestMove FindBestMove(int timeLimit = 1000)
        {
            stopwatch.Restart();

            cancellationTokenSource = new CancellationTokenSource();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                Log.Debug($"thinking time: {timeLimit}");
                Thread.Sleep(timeLimit);
                if (!cancellationTokenSource.IsCancellationRequested && !Debugger.IsAttached)
                {
                    cancellationTokenSource.Cancel();
                }
            });

            if (Board.Counters.HalfmoveClock == 0)
            {
                repetitions.Clear();
                hashTable.Clear();
            }

            var openingMove = GetOpeningBookMove();

            var nodes = new ConcurrentBag<Node>((openingMove != null ? new[] { openingMove } : Board.GetLegalMoves()).Select(m => new Node(Board, m)));

            if (!nodes.Any())
            {
                throw new PatzerException("No valid moves found for this board.");
            }

            var color = Board.ActiveColor.Is(Piece.White) ? "White" : "Black";
            Log.Debug($"Legal moves for {color}: {string.Join(';', nodes.Select(n => n.Move))}");

            var cancellationToken = cancellationTokenSource.Token;

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                if (nodes.Any(n => n.MateIn > 0))
                {
                    break;
                }

                var nodesToAnalyze = nodes.Where(n => !n.MateIn.HasValue);

                if (nodesToAnalyze.Count() <= 1)
                {
                    break;
                }

                var waitingNodes = nodesToAnalyze.Where(n => n.Status == NodeStatus.Waiting).OrderByDescending(n => n.AbsoluteScore);

                var runningNodes = nodesToAnalyze.Count(n => n.Status == NodeStatus.Running);

                Task.WaitAny(waitingNodes.Take(Environment.ProcessorCount-runningNodes).Select(node => Task.Run(() =>
                {
                    try
                    {
                        node.Status = NodeStatus.Running;

                        var evaluation = EvaluateBoard(node.Board, node, 1, -Declarations.MoveMaximumScore, Declarations.MoveMaximumScore, cancellationToken);

                        if (node.Status != NodeStatus.Cancelled)
                        {
                            node.Score = evaluation;

                            var principalVariation = FindPrincipalVariation(Board, node).ToArray();
                            node.PonderMove = principalVariation.Length > 1 ? principalVariation[1] : null;

                            SendAnalysisInfo(node.MaxDepth, nodes.Sum(n => n.Count), node, principalVariation, stopwatch.ElapsedMilliseconds);

                            node.MaxDepth += 2;
                            node.Status = NodeStatus.Waiting;
                        }
                        else
                        {
                            SendDebugInfo($"aborting {node.Move.ToAlgebraicMoveNotation()} @ depth {node.MaxDepth}");

                            Log.Debug($"Discarding evaluation due to timeout: {node.Move} @ depth {node.MaxDepth}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Caught Exception during evaluation.");

                        SendExceptionInfo(ex);

                        throw;
                    }

                })).ToArray());
            }


            // Set node score to zero for threefold repetition moves.
            UpdateForThreefoldRepetition(nodes);

            var bestNode = nodes.OrderByDescending(n => n.AbsoluteScore).First();

            Log.Debug($"evaluated {nodes.Sum(n => n.Count)} nodes, found: {bestNode}");

            return new BestMove(new AlgebraicMove(bestNode.Move), bestNode.PonderMove != null ? new AlgebraicMove(bestNode.PonderMove) : null);
        }

        private IEnumerable<Move> FindPrincipalVariation(IBoard board, Node node)
        {
            var move = node.Move;

            var depth = 0;

            while (depth++ < node.MaxDepth)
            {
                yield return move;

                board = board.Play(move);

                if (!hashTable.TryGetValue(board.Hash, out var pvMove))
                {
                    yield break;
                }

                move = pvMove.Item1;
            }
        }

        private void UpdateForThreefoldRepetition(IReadOnlyCollection<Node> nodes)
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
                    Log.Information($"Resetting score to zero for threefold repetition of: {algebraic}");

                    node.Score = 0;
                }
            }
        }

        private void SendInfo(string info)
        {
            SendCallbackInfo($"info {info}");
        }

        private void SendDebugInfo(string debugInfo)
        {
            SendInfo($"string debug {debugInfo}");
        }

        private void SendExceptionInfo(Exception exception)
        {
            SendInfo($"string exception {exception.GetType().Name} {exception.Message}");
        }

        private void SendAnalysisInfo(int depth, long nodes, Node node, IEnumerable<Move> principalVariation, long time)
        {
            var pv = string.Join(' ', principalVariation.Select(m => m.ToAlgebraicMoveNotation()));

            var score = node.MateIn.HasValue ? $"mate {node.MateIn.Value}" : $"cp {node.AbsoluteScore}";

            var nodesPerSecond = time == 0 ? 0 : nodes * 1000 / time;

            SendInfo($"depth {depth} nodes {nodes} nps {nodesPerSecond} score {score} time {time} pv {pv}");
        }

        private int EvaluateBoard(IBoard board, Node node, int depth, int α, int β, CancellationToken cancellationToken)
        {
            var maximizing = board.ActiveColor.Is(Piece.White);

            var evaluation = maximizing ? -Declarations.MoveMaximumScore : Declarations.MoveMaximumScore;

            if (cancellationToken.IsCancellationRequested)
            {
                node.Status = NodeStatus.Cancelled;

                return evaluation;
            }

            IEnumerable<Move> moves;

            if (hashTable.TryGetValue(board.Hash, out var hashMove))
            {
                moves = board.GetLegalMoves().OrderByDescending(m=>m.Equals(hashMove.Item1));
            }
            else
            {
                moves = board.GetLegalMoves();
            }

            if (!moves.Any())
            {
                var mateScore = maximizing ? -Declarations.MateScore + depth + 1 : Declarations.MateScore - (depth + 1);

                return board.IsChecked ? mateScore : Declarations.DrawScore;
            }

            if (depth == node.MaxDepth)
            {
                return board.Score;
            }

            foreach (var move in moves)
            {
                node.Count++;

                var score = EvaluateBoard(board.Play(move), node, depth + 1, α, β, cancellationToken);

                if (maximizing)
                {
                    if (score > evaluation)
                    {
                        evaluation = score;

                        
                    }

                    if (evaluation > β)
                    {
                        UpdateHashTable(board.Hash, move, evaluation);

                        break;
                    }

                    α = Math.Max(α, evaluation);
                }
                else
                {
                    if (score < evaluation)
                    {
                        evaluation = score;

                        
                    }

                    if (evaluation < α)
                    {
                        UpdateHashTable(board.Hash, move, -evaluation);

                        break;
                    }

                    β = Math.Min(β, evaluation);
                }
            }

            return evaluation;
        }

        private void UpdateHashTable(ulong hash, Move move, int evaluation)
        {
            hashTable.AddOrUpdate(hash, (move, evaluation), (key, old) => {
                if (evaluation < old.Item2)
                {
                    return old;
                }

                return (move, evaluation);
            });
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }
    }
}