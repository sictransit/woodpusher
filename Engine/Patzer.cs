using Serilog;
using SicTransit.Woodpusher.Common.Extensions;
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

        private bool timeIsUp = false;
        private int maxDepth = 0;
        private long nodeCount = 0;

        private readonly CancellationTokenSource cancellationTokenSource = new();

        private readonly Stopwatch stopwatch = new();

        private readonly Dictionary<string, int> repetitions = new();

        private readonly OpeningBook openingBook = new();
        private readonly Action<string>? infoCallback;

        private readonly Dictionary<ulong, (int ply, Move move, int score)> transpositionTable = new();

        private readonly List<(int ply, Move move)> bestLine = new();

        private Move ponderMove;

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

            // All found opening book moves found should be legal moves.
            var legalOpeningBookMoves = openingBookMoves.Select(o => new { openingBookMove = o, legalMove = legalMoves.SingleOrDefault(l => l.Move.ToAlgebraicMoveNotation().Equals(o.Move.Notation)) }).Where(l => l.legalMove != null).ToArray();

            return legalOpeningBookMoves.OrderByDescending(m => m.openingBookMove.Count).FirstOrDefault()?.legalMove?.Move;
        }

        private Move? FindPonderMove()
        {
            if (Board.LastMove == null)
            {
                return null;
            }

            return Board.LastMove.Equals(ponderMove) ? ponderMove : null;
        }

        public void Position(string fen, IEnumerable<AlgebraicMove>? algebraicMoves = null)
        {
            algebraicMoves ??= Array.Empty<AlgebraicMove>();

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

            var openingMove = GetOpeningBookMove();
            if (openingMove != null)
            {
                Log.Information("Returning opening book move: {0}", openingMove);
                return new BestMove(new AlgebraicMove(openingMove));
            }

            var ponderMove = FindPonderMove();
            if (ponderMove != null)
            {
                Log.Information("Returning ponder move: {0}", ponderMove);
                return new BestMove(new AlgebraicMove(ponderMove));
            }

            var sign = Board.ActiveColor.Is(Piece.White) ? 1 : -1;

            var bestMove = default(Move);

            var foundMate = false;

            var enoughTime = true;

            var progress = new List<(int depth, long time)>();

            while (maxDepth < Declarations.MaxDepth - 2 && !foundMate && !timeIsUp && enoughTime)
            {
                try
                {
                    maxDepth += 2;

                    transpositionTable.Clear();

                    // TODO: Check for threefold repetition. Note that we might seek that!

                    long startTime = stopwatch.ElapsedMilliseconds;

                    var (move, score) = EvaluateBoard(Board, 0, -Declarations.MoveMaximumScore, Declarations.MoveMaximumScore, Board.ActiveColor.Is(Piece.White));

                    long evaluationTime = stopwatch.ElapsedMilliseconds - startTime;

                    if (!timeIsUp)
                    {
                        bestMove = move!;

                        UpdateBestLine(bestMove, maxDepth);

                        var nodesPerSecond = stopwatch.ElapsedMilliseconds == 0 ? 0 : nodeCount * 1000 / stopwatch.ElapsedMilliseconds;
                        
                        var mateIn = CalculateMateIn(score, sign);
                        foundMate = mateIn is > 0;
                        var scoreString = mateIn.HasValue ? $"mate {mateIn.Value}" : $"cp {score * sign}";                        

                        var pvString = string.Join(' ', bestLine.Select(m => m.move.ToAlgebraicMoveNotation()));
                        SendInfo($"depth {maxDepth} nodes {nodeCount} nps {nodesPerSecond} score {scoreString} time {stopwatch.ElapsedMilliseconds} pv {pvString}");

                        if (evaluationTime > 0)
                        {
                            progress.Add((maxDepth, evaluationTime));

                            if (progress.Count > 2)
                            {
                                var estimatedTime = MathExtensions.ApproximateNextDepthTime(progress);
                                var remainingTime = timeLimit - stopwatch.ElapsedMilliseconds;

                                enoughTime = remainingTime > estimatedTime;

                                Log.Debug("Estimated time for next depth: {0}ms, remaining time: {1}ms, enough time: {2}", estimatedTime, remainingTime, enoughTime);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Caught Exception during evaluation.");

                    SendExceptionInfo(ex);

                    throw;
                }
            }

            SendDebugInfo($"aborting @ depth {maxDepth}");

            Log.Debug($"evaluated {nodeCount} nodes, found: {bestMove}");


            if (bestMove == null)
            {
                throw new InvalidOperationException("No move found.");
            }

            ponderMove = bestLine.Count>1 ? bestLine[1].move : null;

            return new BestMove(new AlgebraicMove(bestMove), ponderMove != null ? new AlgebraicMove(ponderMove) : null);
        }

        private void UpdateBestLine(Move bestMove, int depth)
        {
            var ply = Board.Counters.Ply +1;

            bestLine.Clear();
            bestLine.Add((ply, bestMove));

            var board = Board.Play(bestMove);

            for (var i = 0; i < depth; i++)
            {
                if (transpositionTable.TryGetValue(board.Hash, out var cached))
                {
                    bestLine.Add((ply + i + 1, cached.move));

                    board = board.Play(cached.move);
                }
                else
                {
                    break;
                }
            }
        }

        private int? CalculateMateIn(int evaluation, int sign)
        {
            var mateIn = Math.Abs(Math.Abs(evaluation) - Declarations.MateScore) - Board.Counters.Ply;

            if (mateIn <= Declarations.MaxDepth)
            {

                return sign * (mateIn / 2 + (sign > 0 ? 1 : 0));
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

        private (Move? move, int score) EvaluateBoard(IBoard board, int depth, int α, int β, bool maximizing)
        {
            if (depth == maxDepth || timeIsUp)
            {
                return (board.LastMove, board.Score * (maximizing ? 1 : -1));
            }

            // TODO: Fix! This breaks mate detection somehow.
            if (transpositionTable.TryGetValue(board.Hash, out var cached) && cached.ply == board.Counters.Ply)
            {
                //Log.Debug($"Transposition table hit: {board.Hash} {cached.move} {cached.score}");

                return (cached.move, cached.score);
            }

            Move? bestMove = default;

            var bestScore = -Declarations.MoveMaximumScore;

            var legalMoves = board.GetLegalMoves();
            //var legalMoves = board.GetLegalMoves().OrderByDescending(l => transpositionTable.ContainsKey(l.Board.Hash)).ThenByDescending(l=>l.Board.Counters.Capture != Piece.None);

            foreach (var legalMove in legalMoves)
            {
                nodeCount++;

                var (_, score) = EvaluateBoard(legalMove.Board, depth + 1, -β, -α, !maximizing);
                score = -score;

                if (score > bestScore)
                {
                    bestMove = legalMove.Move;
                    bestScore = score;
                }

                α = Math.Max(α, bestScore);

                if (α >= β)
                {
                    break;
                }
            }

            if (bestMove == default)
            {
                bestMove = board.LastMove;

                if (board.IsChecked)
                {
                    bestScore = -Declarations.MateScore + board.Counters.Ply;
                }
                else
                {
                    bestScore = Declarations.DrawScore;
                }
            }
            
            transpositionTable[board.Hash] = (board.Counters.Ply, bestMove, bestScore);

            return (bestMove, bestScore);
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            timeIsUp = true;
        }
    }
}