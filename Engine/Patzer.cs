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
using static System.Formats.Asn1.AsnWriter;
using System.Xml.Linq;
using System.Numerics;

namespace SicTransit.Woodpusher.Engine
{
    public class Patzer : IEngine
    {
        public IBoard Board { get; private set; }

        private readonly Random random = new();

        private bool timeIsUp = false;
        private int maxDepth = 0;
        private long nodeCount = 0;

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

            timeIsUp = false;
            maxDepth = 0;
            nodeCount = 0;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                Log.Debug($"thinking time: {timeLimit}");
                Thread.Sleep(timeLimit);
                timeIsUp = true;
            });

            if (Board.Counters.HalfmoveClock == 0)
            {
                repetitions.Clear();                
            }

            // TODO: Use the opening book!
            //var openingMove = GetOpeningBookMove();

            var sign = Board.ActiveColor.Is(Piece.White) ? 1 : -1;

            (Move?, int) bestEvaluation = (default, -sign*Declarations.MoveMaximumScore);

            var foundMate = false;

            while (maxDepth < Declarations.MaxDepth-2 && !foundMate)
            {
                try
                {
                    maxDepth += 2;

                    // TODO: Check for threefold repetition. Note the we might seek that!

                    var evaluation = EvaluateBoard(Board,  0, -Declarations.MoveMaximumScore, Declarations.MoveMaximumScore);

                    if (!timeIsUp)
                    {
                        // 15 is better than 10 for white, but -15 is better than -10 for black.
                        if (Math.Sign(evaluation.Item2 - bestEvaluation.Item2) == sign)
                        { 
                            bestEvaluation = evaluation;
                        }

                        var nodesPerSecond = stopwatch.ElapsedMilliseconds == 0 ? 0 : nodeCount * 1000 / stopwatch.ElapsedMilliseconds;
                        var mateIn = GenerateScoreString(evaluation.Item2, sign);
                        foundMate = mateIn is > 0;

                        var scoreString = mateIn.HasValue ? $"mate {mateIn.Value}" : $"score cp {evaluation.Item2 * sign}";
                        
                        var pvString = evaluation.Item1.ToAlgebraicMoveNotation();
                        SendInfo($"depth {maxDepth} nodes {nodeCount} nps {nodesPerSecond} {scoreString} time {stopwatch.ElapsedMilliseconds} pv {pvString}");
                    }
                    
                    else
                    {
                        SendDebugInfo($"aborting @ depth {maxDepth}");    
                        
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Caught Exception during evaluation.");

                    SendExceptionInfo(ex);

                    throw;
                }
            }

            var bestMove = bestEvaluation.Item1;

            Log.Debug($"evaluated {nodeCount} nodes, found: {bestMove}");

            // TODO! Return ponder move.

            return new BestMove(new AlgebraicMove(bestMove));
        }

        private static int? GenerateScoreString(int evaluation, int sign)
        {
            var mateIn = Math.Abs(Math.Abs(evaluation) - Declarations.MateScore);

            if (mateIn <= Declarations.MaxDepth)
            {
                var mateSign = evaluation * sign > 0 ? 1 : -1;

                return mateSign * mateIn / 2;
            }

            return null;
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

        private (Move?,int) EvaluateBoard(IBoard board, int depth, int α, int β)
        {
            var maximizing = board.ActiveColor.Is(Piece.White);

            if (depth == maxDepth || timeIsUp)
            {
                return (board.LastMove, board.Score);
            }

            Move? bestMove = default;

            var evaluation = maximizing ? -Declarations.MoveMaximumScore : Declarations.MoveMaximumScore;

            foreach (var legalMove in board.GetLegalMoves())
            {
                nodeCount++;

                var score = EvaluateBoard(legalMove.Board, depth + 1, α, β);

                if (maximizing)
                {
                    if (score.Item2 > evaluation)
                    {
                        bestMove = legalMove.Move;
                        evaluation = score.Item2;
                    }

                    α = Math.Max(α, evaluation);

                    if (α >= β)
                    {
                        break;
                    }                    
                }
                else
                {
                    if (score.Item2 < evaluation)
                    {
                        bestMove = legalMove.Move;
                        evaluation = score.Item2;
                    }

                    β = Math.Min(β, evaluation);

                    if (β <= α)
                    {
                        break;
                    }                    
                }
            }

            if (bestMove == default)
            {
                var mateScore = maximizing ? -Declarations.MateScore + depth + 1 : Declarations.MateScore - (depth + 1);

                return (board.LastMove, board.IsChecked ? mateScore : Declarations.DrawScore);
            }

            return (bestMove, evaluation);
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            timeIsUp = true;
        }
    }
}