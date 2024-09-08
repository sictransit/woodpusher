using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Common.Exceptions;
using SicTransit.Woodpusher.Common.Extensions;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Lookup;
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

        private readonly Dictionary<string, int> repetitions = [];
        
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

            var legalOpeningBookMoves = openingBookMoves.Select(o => new { openingBookMove = o, legalMove = legalMoves.SingleOrDefault(l => l.Move.ToAlgebraicMoveNotation().Equals(o.Move.Notation)) }).Where(l => l.legalMove != null).ToArray();

            return legalOpeningBookMoves.OrderByDescending(m => random.Next(m.openingBookMove.Count)).ThenByDescending(_ => random.NextDouble()).FirstOrDefault()?.legalMove.Move;
        }

        public void Position(string fen, IEnumerable<AlgebraicMove>? algebraicMoves = null)
        {
            algebraicMoves ??= [];

            Board = ForsythEdwardsNotation.Parse(fen);

            foreach (var algebraicMove in algebraicMoves)
            {
                var legalMove = Board.GetLegalMoves().SingleOrDefault(l => l.Move.Piece.GetSquare().Equals(algebraicMove.From) && l.Move.GetTarget().Equals(algebraicMove.To) && l.Move.PromotionType == algebraicMove.Promotion);

                if (legalMove == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(algebraicMoves), $"unable to play: {algebraicMove}");
                }

                Play(legalMove.Move);
            }
        }

        public void Perft(int depth)
        {
            cancellationTokenSource = new CancellationTokenSource();

            var initialMoves = Board.GetLegalMoves();

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
                    nodes += i.Board.Perft(depth);
                }
                else
                {
                    nodes = 1;
                }

                Interlocked.Add(ref total, nodes);

                SendCallbackInfo($"{i.Move.ToAlgebraicMoveNotation()}: {nodes}");
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
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();
                }
            });

            if (Board.Counters.HalfmoveClock == 0)
            {
                repetitions.Clear();                
            }

            var openingMove = GetOpeningBookMove();

            var nodes = new ConcurrentBag<Node>((openingMove != null ? [openingMove] : Board.GetLegalMoves().Select(l => l.Move)).Select(m => new Node(Board, m)));

            if (nodes.IsEmpty)
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

                var nodesToAnalyze = nodes.Where(n => !n.MateIn.HasValue).OrderByDescending(n => n.Improvement).ToArray();

                if (nodesToAnalyze.Length <= 1)
                {
                    break;
                }

                foreach(var node in nodesToAnalyze) 
                {
                    try
                    {
                        var evaluation = EvaluateBoard(node.Board, node, 1, -Declarations.MoveMaximumScore, Declarations.MoveMaximumScore, cancellationToken);

                        if (!node.Cancelled)
                        {
                            node.Score = evaluation;

                            SendAnalysisInfo(node.MaxDepth, nodes.Sum(n => n.Count), node, stopwatch.ElapsedMilliseconds);

                            node.MaxDepth += 2;                                
                        }
                        else
                        {
                            node.Cancelled = false;

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
                }
            }


            // Set node score to zero for threefold repetition moves.
            UpdateForThreefoldRepetition(nodes);

            var bestNode = nodes.OrderByDescending(n => n.AbsoluteScore).First();

            Log.Debug($"evaluated {nodes.Sum(n => n.Count)} nodes, found: {bestNode}");

            return new BestMove(new AlgebraicMove(bestNode.Move), bestNode.PonderMove != null ? new AlgebraicMove(bestNode.PonderMove) : null);
        }

        private void UpdateForThreefoldRepetition(IReadOnlyCollection<Node> nodes)
        {
            foreach (var node in nodes)
            {
                var algebraic = node.Move.ToAlgebraicMoveNotation();
                var key = $"{Board.Hash}_{algebraic}";

                if (!repetitions.TryAdd(key, 1))
                {
                    repetitions[key] += 1;
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

        private void SendAnalysisInfo(int depth, long nodes, Node node, long time)
        {
            var score = node.MateIn.HasValue ? $"mate {node.MateIn.Value}" : $"cp {node.AbsoluteScore}";

            var nodesPerSecond = time == 0 ? 0 : nodes * 1000 / time;

            SendInfo($"depth {depth} nodes {nodes} nps {nodesPerSecond} score {score} time {time} pv {node.Move.ToAlgebraicMoveNotation()}");
        }

        private int EvaluateBoard(IBoard board, Node node, int depth, int α, int β, CancellationToken cancellationToken)
        {
            var maximizing = board.ActiveColor.Is(Piece.White);

            var evaluation = maximizing ? -Declarations.MoveMaximumScore : Declarations.MoveMaximumScore;

            if (cancellationToken.IsCancellationRequested)
            {
                node.Cancelled = true;

                return evaluation;
            }

            IEnumerable<LegalMove> legalMoves = board.GetLegalMoves();            

            if (!legalMoves.Any())
            {
                var mateScore = maximizing ? -Declarations.MateScore + depth + 1 : Declarations.MateScore - (depth + 1);

                return board.IsChecked ? mateScore : Declarations.DrawScore;
            }

            if (depth == node.MaxDepth)
            {
                return board.Score;
            }

            foreach (var legalMove in legalMoves)
            {
                node.Count++;

                var score = EvaluateBoard(legalMove.Board, node, depth + 1, α, β, cancellationToken);

                if (maximizing)
                {
                    if (score > evaluation)
                    {
                        evaluation = score;
                    }

                    if (evaluation > β)
                    {
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
                        break;
                    }

                    β = Math.Min(β, evaluation);
                }
            }

            return evaluation;
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }
    }
}