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

        private const int transpositionTableSize = 1_000_000;

        private readonly TranspositionTableEntry[] transpositionTable = new TranspositionTableEntry[transpositionTableSize];

        private readonly List<(int ply, Move move)> bestLine = new();

        private Move? evaluatedBestMove = null;

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
            var legalOpeningBookMoves = openingBookMoves.Select(o => new { openingBookMove = o, legalMove = legalMoves.SingleOrDefault(move => move.ToAlgebraicMoveNotation().Equals(o.Move.Notation)) }).Where(l => l.legalMove != null).ToArray();

            return legalOpeningBookMoves.OrderByDescending(m => m.openingBookMove.Count).FirstOrDefault()?.legalMove;
        }

        public void Position(string fen, IEnumerable<AlgebraicMove>? algebraicMoves = null)
        {
            algebraicMoves ??= Array.Empty<AlgebraicMove>();

            Board = ForsythEdwardsNotation.Parse(fen);

            foreach (var algebraicMove in algebraicMoves)
            {
                var legalMove = Board.GetLegalMoves().SingleOrDefault(move => move.Piece.GetSquare().Equals(algebraicMove.From) && move.GetTarget().Equals(algebraicMove.To) && move.PromotionType == algebraicMove.Promotion);

                if (legalMove == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(algebraicMoves), $"unable to play: {algebraicMove}");
                }

                Play(legalMove);
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
                    nodes += Board.Play(i).Perft(depth);
                }
                else
                {
                    nodes = 1;
                }

                Interlocked.Add(ref total, nodes);

                SendCallbackInfo($"{i.ToAlgebraicMoveNotation()}: {nodes}");
            });

            SendCallbackInfo(Environment.NewLine + $"Nodes searched: {total}");
        }

        public AlgebraicMove FindBestMove(int timeLimit = 1000)
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

            // TODO: Fill bestLine with the opening book moves.
            var openingMove = GetOpeningBookMove();
            if (openingMove != null)
            {
                Log.Information("Returning opening book move: {0}", openingMove);
                SendDebugInfo($"playing opening book {openingMove.ToAlgebraicMoveNotation()}");
                return new AlgebraicMove(openingMove);
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

                    Array.Clear(transpositionTable, 0, transpositionTable.Length);

                    // TODO: Check for threefold repetition. Note that we might seek that!

                    long startTime = stopwatch.ElapsedMilliseconds;

                    var score = EvaluateBoard(Board, 0, -Declarations.MoveMaximumScore, Declarations.MoveMaximumScore, sign);

                    long evaluationTime = stopwatch.ElapsedMilliseconds - startTime;

                    if (!timeIsUp)
                    {
                        bestMove = evaluatedBestMove;

                        bestLine.Clear();
                        if (bestMove != null)
                        {
                            UpdateBestLine(bestMove, maxDepth);
                        }

                        var nodesPerSecond = stopwatch.ElapsedMilliseconds == 0 ? 0 : nodeCount * 1000 / stopwatch.ElapsedMilliseconds;

                        var mateIn = CalculateMateIn(score, sign);
                        foundMate = mateIn is > 0;

                        var scoreString = mateIn.HasValue ? $"mate {mateIn.Value}" : $"cp {score}";
                        var pvString = string.Join(' ', bestLine.Select(m => m.move.ToAlgebraicMoveNotation()));                        
                        var hashFull = transpositionTable.Count(t=>t.Hash != 0)*1000 / transpositionTableSize;
                        SendInfo($"depth {maxDepth} nodes {nodeCount} nps {nodesPerSecond} hashfull {hashFull} score {scoreString} time {stopwatch.ElapsedMilliseconds} pv {pvString}");

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

            return new AlgebraicMove(bestMove);
        }

        private void UpdateBestLine(Move bestMove, int depth)
        {
            var ply = Board.Counters.Ply + 1;

            bestLine.Add((ply, bestMove));

            var board = Board.Play(bestMove);

            for (var i = 0; i < depth; i++)
            {
                var entry = transpositionTable[board.Hash % transpositionTableSize];
                if (entry.Hash == board.Hash && entry.Move != null && board.GetLegalMoves().Contains(entry.Move))
                {
                    bestLine.Add((ply + i + 1, entry.Move));

                    board = board.Play(entry.Move);
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

        private int EvaluateBoard(IBoard board, int depth, int α, int β, int sign)
        {
            if (timeIsUp)
            {
                return 0;
            }

            var boards = board.PlayLegalMoves().OrderByDescending(b => b.Counters.Capture != Piece.None).ToArray();

            if (boards.Length == 0)
            {
                if (board.IsChecked)
                {
                    return -Declarations.MateScore + board.Counters.Ply;
                }
                else
                {
                    return Declarations.DrawScore;
                }
            }

            if (depth == maxDepth)
            {
                return board.Score * sign;
            }

            var transpositionIndex = board.Hash % transpositionTableSize;

            var cachedEntry = transpositionTable[transpositionIndex];

            if (cachedEntry.Hash == board.Hash && cachedEntry.Ply == board.Counters.Ply)
            {
                return cachedEntry.Score;
            }

            Move? bestMove = null;

            var bestScore = -Declarations.MoveMaximumScore;

            foreach (var newBoard in boards)
            {
                nodeCount++;

                var score = -EvaluateBoard(newBoard, depth + 1, -β, -α, -sign);

                if (score > bestScore)
                {
                    bestMove = newBoard.Counters.LastMove;
                    bestScore = score;
                }

                α = Math.Max(α, bestScore);

                if (α >= β)
                {
                    break;
                }
            }

            var entry = transpositionTable[transpositionIndex];

            if (entry.Ply <= board.Counters.Ply || (entry.Ply == board.Counters.Ply && entry.Score < bestScore))
            {
                transpositionTable[transpositionIndex] = new TranspositionTableEntry(board.Counters.Ply, bestMove, bestScore, board.Hash);
            }

            evaluatedBestMove = bestMove;

            return bestScore;
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            timeIsUp = true;
        }
    }
}