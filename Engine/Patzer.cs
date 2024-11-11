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

        private readonly Stopwatch stopwatch = new();

        private readonly Dictionary<ulong, int> repetitions = [];

        private OpeningBook openingBook;
        private readonly Action<string>? infoCallback;

        private const int transpositionTableSize = 1_000_000;

        private readonly TranspositionTableEntry[] transpositionTable = new TranspositionTableEntry[transpositionTableSize];

        private readonly List<(int ply, Move move)> bestLine = [];

        private Move? evaluatedBestMove = null;

        public Patzer(Action<string>? infoCallback = null)
        {
            Board = Common.Board.StartingPosition();

            this.infoCallback = infoCallback;
        }

        public void Initialize()
        {
            Board = Common.Board.StartingPosition();

            repetitions.Clear();
            repetitions.Add(Board.Hash, 1);
        }

        private void SendCallbackInfo(string info) => infoCallback?.Invoke(info);

        public void Play(Move move)
        {
            var color = Board.ActiveColor.Is(Piece.White) ? "White" : "Black";

            Log.Debug("{Color} plays: {Move}", color, move);

            Board = Board.Play(move);

            repetitions[Board.Hash] = repetitions.GetValueOrDefault(Board.Hash) + 1;
        }

        private Move? GetOpeningBookMove()
        {
            if (openingBook == null)
            {
                openingBook = new OpeningBook(Board.ActiveColor);
            }

            var openingBookMoves = openingBook.GetMoves(Board.Hash);

            var legalMoves = Board.GetLegalMoves().ToArray();

            // All found opening book moves found should be legal moves.
            var legalOpeningBookMoves = openingBookMoves.Select(o => new { openingBookMove = o, legalMove = legalMoves.SingleOrDefault(move => move.ToAlgebraicMoveNotation().Equals(o.Move.Notation)) }).Where(l => l.legalMove != null).ToArray();

            return legalOpeningBookMoves.OrderByDescending(m => m.openingBookMove.Count).FirstOrDefault()?.legalMove;
        }

        private Move? GetTheoryMove()
        {
            var theoryMoves = Board.PlayLegalMoves().Select(b => new { move = b.Counters.LastMove, count = openingBook.GetMoves(b.Hash).Count() }).Where(t => t.count > 0).ToArray();

            return theoryMoves.OrderByDescending(m => m.count).FirstOrDefault()?.move;
        }

        public void Position(string fen, IEnumerable<AlgebraicMove>? algebraicMoves = null)
        {
            algebraicMoves ??= [];

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
            ulong nodes = 0;

            foreach (var board in Board.PlayLegalMoves())
            {
                if (depth > 1)
                {
                    nodes += board.Perft(depth);
                }
                else
                {
                    nodes += 1;
                }
            }

            SendCallbackInfo(Environment.NewLine + $"Nodes searched: {nodes}");
        }

        public AlgebraicMove FindBestMove(int timeLimit = 1000)
        {
            stopwatch.Restart();

            timeIsUp = false;
            maxDepth = 0;
            nodeCount = 0;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                Log.Debug("thinking time: {TimeLimit}", timeLimit);
                Thread.Sleep(timeLimit);
                timeIsUp = true;
            });

            // TODO: Fill bestLine with the opening book moves.
            var openingMove = GetOpeningBookMove();
            if (openingMove != null)
            {
                Log.Information("Returning opening book move: {0}", openingMove);
                SendDebugInfo($"playing opening book {openingMove.ToAlgebraicMoveNotation()}");
                return new AlgebraicMove(openingMove);
            }

            var theoryMove = GetTheoryMove();
            if (theoryMove != null)
            {
                Log.Information("Returning theory move: {0}", theoryMove);
                SendDebugInfo($"playing theory move {theoryMove.ToAlgebraicMoveNotation()}");
                return new AlgebraicMove(theoryMove);
            }

            var sign = Board.ActiveColor.Is(Piece.White) ? 1 : -1;

            var bestMove = default(Move);
            var foundMate = false;
            var enoughTime = true;

            var progress = new List<(int depth, long time)>();

            Array.Clear(transpositionTable, 0, transpositionTable.Length);

            while (maxDepth < Declarations.MaxDepth - 2 && !foundMate && !timeIsUp && enoughTime)
            {
                try
                {
                    maxDepth += 2;

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
                        var hashFull = transpositionTable.Count(t => t.Hash != 0) * 1000 / transpositionTableSize;
                        SendInfo($"depth {maxDepth} nodes {nodeCount} nps {nodesPerSecond} hashfull {hashFull} score {scoreString} time {stopwatch.ElapsedMilliseconds} pv {pvString}");

                        if (evaluationTime > 0)
                        {
                            progress.Add((maxDepth, evaluationTime));
                            if (progress.Count > 3)
                            {
                                progress.RemoveAt(0);
                            }

                            // Don't try to estimate enought time unless we've got >2 points, i.e. we're at least at depth 6.
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
                    SendExceptionInfo(ex);

                    throw;
                }
            }

            SendDebugInfo($"aborting @ depth {maxDepth}");

            Log.Debug("evaluated {NodeCount} nodes, found: {BestMove}", nodeCount, bestMove);

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

        private void SendInfo(string info) => SendCallbackInfo($"info {info}");

        private void SendDebugInfo(string debugInfo) => SendInfo($"string debug {debugInfo}");

        private void SendExceptionInfo(Exception exception) => SendInfo($"string exception {exception.GetType().Name} {exception.Message}");

        private int EvaluateBoard(IBoard board, int depth, int α, int β, int sign)
        {
            if (timeIsUp)
            {
                return 0;
            }

            if (depth == maxDepth)
            {
                if (repetitions.GetValueOrDefault(board.Hash) >= 2)
                {
                    return Declarations.DrawScore;
                }

                return board.Score * sign;
            }

            var boards = board.PlayLegalMoves().OrderByDescending(b => b.Score * sign).ToArray();

            if (boards.Length == 0)
            {
                return board.IsChecked ? -Declarations.MateScore + board.Counters.Ply : Declarations.DrawScore;
            }

            var transpositionIndex = board.Hash % transpositionTableSize;

            var cachedEntry = transpositionTable[transpositionIndex];

            if (cachedEntry.Hash == board.Hash)
            {
                if (cachedEntry.MaxDepth == maxDepth && cachedEntry.Ply == board.Counters.Ply)
                {
                    return cachedEntry.Score;
                }

                var moveIndex = Array.FindIndex(boards, b => b.Counters.LastMove.Equals(cachedEntry.Move));

                if (moveIndex > 0)
                {
                    (boards[moveIndex], boards[0]) = (boards[0], boards[moveIndex]);
                }
            }

            Move? bestMove = null;

            var bestScore = -Declarations.MoveMaximumScore;

            foreach (var newBoard in boards)
            {
                nodeCount++;

#pragma warning disable S2234 // Arguments should be passed in the same order as the method parameters
                var score = -EvaluateBoard(newBoard, depth + 1, -β, -α, -sign);
#pragma warning restore S2234 // Arguments should be passed in the same order as the method parameters

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

            transpositionTable[transpositionIndex] = new TranspositionTableEntry(board.Counters.Ply, bestMove, bestScore, board.Hash, maxDepth);

            evaluatedBestMove = bestMove;

            return bestScore;
        }

        public void Stop() => timeIsUp = true;
    }
}