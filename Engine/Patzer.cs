using Serilog;
using SicTransit.Woodpusher.Common;
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
        private int selDepth = 0;
        private uint nodeCount = 0;
        private const uint EngineMaxDepth = 128;

        private OpeningBook? openingBook;

        private readonly Action<string>? infoCallback;

        private const int transpositionTableSize = 1_000_000;

        private readonly TranspositionTableEntry[] transpositionTable = new TranspositionTableEntry[transpositionTableSize];
        private readonly Dictionary<ulong, int> repetitionTable = [];
        private readonly ulong[][] killerMoves = new ulong[1000][]; // TODO: Phase out killer moves as the game progresses.

        private readonly List<(int ply, Move move)> bestLine = [];

        private Move? evaluatedBestMove = null;

        private EngineOptions engineOptions;

        public Patzer(Action<string>? infoCallback = null)
        {
            this.infoCallback = infoCallback;

            Initialize(EngineOptions.Default);
        }

        public void Initialize(EngineOptions options)
        {
            engineOptions = options;

            SetBoard(Common.Board.StartingPosition());

            openingBook = null;

            for (var i = 0; i < killerMoves.Length; i++)
            {
                killerMoves[i] = new ulong[2];
            }
        }

        private void SetBoard(IBoard board)
        {
            Board = board;

            repetitionTable.Clear();
            repetitionTable[Board.Hash] = 1;
        }

        private void SendCallbackInfo(string info) => infoCallback?.Invoke(info);

        public void Play(Move move)
        {
            var color = Board.ActiveColor.Is(Piece.White) ? "White" : "Black";
            Log.Debug("{Color} plays: {Move}", color, move);
            Board = Board.Play(move);

            if (Board.Counters.HalfmoveClock == 0)
            {
                repetitionTable.Clear();
            }

            repetitionTable[Board.Hash] = repetitionTable.GetValueOrDefault(Board.Hash) + 1;
        }

        private Move? GetOpeningBookMove()
        {
            openingBook ??= Board.ActiveColor == Piece.White ? new OpeningBook(Piece.White) : new OpeningBook(Piece.None);

            var openingBookMoves = openingBook.GetMoves(Board.Hash);
            var legalMoves = Board.GetLegalMoves().ToArray();
            var legalOpeningBookMoves = openingBookMoves
                .Select(o => new { openingBookMove = o, legalMove = legalMoves.SingleOrDefault(move => move.ToAlgebraicMoveNotation().Equals(o.Move.Notation)) })
                .Where(l => l.legalMove != null)
                .ToArray();
            return legalOpeningBookMoves.OrderByDescending(m => m.openingBookMove.Count).FirstOrDefault()?.legalMove;
        }

        private Move? GetTheoryMove()
        {
            openingBook ??= Board.ActiveColor == Piece.White ? new OpeningBook(Piece.White) : new OpeningBook(Piece.None);

            var theoryMoves = Board.PlayLegalMoves()
                .Select(b => new { move = b.Counters.LastMove, count = openingBook?.GetMoves(b.Hash).Count() ?? 0 })
                .Where(t => t.count > 0)
                .ToArray();
            return theoryMoves.OrderByDescending(m => m.count).FirstOrDefault()?.move;
        }

        public void Position(string fen, IEnumerable<AlgebraicMove>? algebraicMoves = null)
        {
            algebraicMoves ??= Array.Empty<AlgebraicMove>();

            SetBoard(ForsythEdwardsNotation.Parse(fen));

            foreach (var algebraicMove in algebraicMoves)
            {
                var legalMove = Board.GetLegalMoves().SingleOrDefault(move =>
                    move.Piece.GetSquare().Equals(algebraicMove.From) &&
                    move.GetTarget().Equals(algebraicMove.To) &&
                    move.PromotionType == algebraicMove.Promotion);

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
                nodes += depth > 1 ? board.Perft(depth) : 1;
            }
            SendCallbackInfo(Environment.NewLine + $"Nodes searched: {nodes}");
        }

        public AlgebraicMove? FindBestMove(int timeLimit = 1000)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            timeIsUp = false;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    if (!token.WaitHandle.WaitOne(timeLimit))
                    {
                        Log.Information("Time is up: {ElapsedMilliseconds}", stopwatch.ElapsedMilliseconds);
                        timeIsUp = true;
                    }
                }
                catch (OperationCanceledException)
                {
                    Log.Debug("Time limit check was canceled.");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Time limit check failed.");
                }
            }, token);

            AlgebraicMove? bestMove;

            try
            {
                Log.Information("Thinking time: {TimeLimit} ms", timeLimit);

                bestMove = SearchForBestMove(stopwatch, timeLimit);
            }
            catch (Exception ex)
            {
                SendExceptionInfo(ex);
                throw;
            }
            finally
            {
                cts.Cancel();
            }

            return bestMove;
        }

        private void AddKillerMove(int ply, ulong hash)
        {
            killerMoves[ply][1] = killerMoves[ply][0];
            killerMoves[ply][0] = hash;
        }

        private AlgebraicMove? SearchForBestMove(Stopwatch stopwatch, int timeLimit = 1000)
        {
            maxDepth = 0;
            nodeCount = 0;
            Move? bestMove = null;
            var enoughTime = true;
            var progress = new List<(int depth, long time)>();
            Array.Clear(transpositionTable, 0, transpositionTable.Length);

            if (engineOptions.UseOpeningBook)
            {
                var bookMove = GetOpeningBookMove() ?? GetTheoryMove();
                if (bookMove != null)
                {
                    UpdateBestLine(bookMove);
                    Log.Information("Returning book move: {0}", bookMove);
                    SendDebugInfo($"playing book move {bookMove.ToAlgebraicMoveNotation()}");
                    SendProgress(stopwatch, 0, null);
                    return new AlgebraicMove(bookMove);
                }
            }

            while (maxDepth < EngineMaxDepth)
            {
                maxDepth++;
                selDepth = maxDepth;
                long startTime = stopwatch.ElapsedMilliseconds;
                int? mateIn = default;

                var score = EvaluateBoard(Board, 0, -Scoring.MoveMaximumScore, Scoring.MoveMaximumScore, Board.ActiveColor.Is(Piece.White) ? 1 : -1);

                long evaluationTime = stopwatch.ElapsedMilliseconds - startTime;

                if (!timeIsUp || (bestMove == null))
                {
                    bestMove = evaluatedBestMove;

                    if (bestMove != null)
                    {
                        UpdateBestLine(bestMove);
                    }

                    if (Math.Abs(score) == Scoring.MateScore)
                    {
                        mateIn = maxDepth / 2 * Math.Sign(score);
                    }

                    SendProgress(stopwatch, score, mateIn);

                    if (evaluationTime > 0)
                    {
                        progress.Add((maxDepth, evaluationTime));

                        if (progress.Count > 2)
                        {
                            var estimatedTime = MathExtensions.ApproximateNextDepthTime(progress, maxDepth + 1);
                            var remainingTime = timeLimit - stopwatch.ElapsedMilliseconds;
                            enoughTime = remainingTime > estimatedTime;
                            Log.Debug("Estimated time for next depth: {0}ms, remaining time: {1}ms, enough time: {2}", estimatedTime, remainingTime, enoughTime);
                        }
                    }
                }

                string? abortMessage = null;

                if (mateIn.HasValue)
                {
                    abortMessage = $"aborting search @ depth {maxDepth}, mate in {mateIn}";
                }
                else if (timeIsUp)
                {
                    abortMessage = $"aborting search @ depth {maxDepth}, time is up";
                }
                else if (!enoughTime)
                {
                    abortMessage = $"aborting search @ depth {maxDepth}, not enough time";
                }

                if (abortMessage != null)
                {
                    SendDebugInfo(abortMessage);
                    break;
                }
            }

            Log.Debug("evaluated {NodeCount} nodes, found: {BestMove}", nodeCount, bestMove);

            return bestMove == null ? null : new AlgebraicMove(bestMove);
        }

        private void SendProgress(Stopwatch stopwatch, int score, int? mateIn)
        {
            var nodesPerSecond = stopwatch.ElapsedMilliseconds == 0 ? 0 : nodeCount * 1000 / stopwatch.ElapsedMilliseconds;
            var hashFull = transpositionTable.Count(t => t.Hash != 0) * 1000 / transpositionTableSize;
            var scoreString = mateIn.HasValue ? $"mate {mateIn.Value}" : $"cp {score}";
            var pvString = string.Join(' ', bestLine.Select(m => m.move.ToAlgebraicMoveNotation()));

            SendInfo($"depth {maxDepth} seldepth {selDepth} nodes {nodeCount} nps {nodesPerSecond} hashfull {hashFull} score {scoreString} time {stopwatch.ElapsedMilliseconds} pv {pvString}");
        }

        private void SendCurrentMove(IBoard board, int currentMoveNumber)
        {
            SendInfo($"depth {maxDepth} currmove {board.Counters.LastMove.ToAlgebraicMoveNotation()} currmovenumber {currentMoveNumber}");
        }

        private void UpdateBestLine(Move bestMove)
        {
            var ply = Board.Counters.Ply + 1;

            bestLine.Clear();
            bestLine.Add((ply, bestMove));

            var board = Board.Play(bestMove);

            for (var i = 0; i < maxDepth; i++)
            {
                var entry = transpositionTable[board.Hash % transpositionTableSize];
                if (entry.EntryType == Enum.EntryType.Exact && entry.Hash == board.Hash && board.GetLegalMoves().Contains(entry.Move))
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

        private void SendInfo(string info) => SendCallbackInfo($"info {info}");

        private void SendDebugInfo(string debugInfo) => SendInfo($"string debug {debugInfo}");

        private void SendExceptionInfo(Exception exception) => SendInfo($"string exception {exception.GetType().Name} {exception.Message}");

        private IEnumerable<IBoard> SortBoards(IEnumerable<IBoard> boards, Move? preferredMove = null)
        {
            return boards.OrderByDescending(board =>
            {
                if (preferredMove != null && board.Counters.LastMove.Equals(preferredMove))
                {
                    return 20; // Highest priority for preferred move.
                }

                if (transpositionTable[board.Hash % transpositionTableSize].EntryType == Enum.EntryType.Exact)
                {
                    return 10; // High priority for exact transposition table entries.
                }

                if (board.Counters.Capture != Piece.None)
                {
                    return Scoring.GetBasicBieceValue(board.Counters.Capture) - Scoring.GetBasicBieceValue(board.Counters.LastMove.Piece); // Capture value, sorting valuable captures first.
                }

                if (killerMoves[board.Counters.Ply].Contains(board.Hash))
                {
                    return -10; // High priority for killer moves
                }

                if (board.IsChecked)
                {
                    return -15;
                }

                //if (board.Counters.LastMove.Flags.HasFlag(SpecialMove.PawnPromotes))
                //{
                //    return -20;
                //}

                return -25;
            });
        }

        private int Quiesce(IBoard board, int α, int β, int sign)
        {
            var standPat = board.Score * sign;

            if (standPat >= β)
            {
                return β;
            }

            α = Math.Max(α, standPat);

            selDepth = Math.Max(selDepth, board.Counters.Ply - Board.Counters.Ply);

            foreach (var newBoard in SortBoards(board.PlayLegalMoves(true)))
            {
                nodeCount++;
                var score = -Quiesce(newBoard, -β, -α, -sign);
                if (score >= β)
                {
                    return β;
                }
                α = Math.Max(α, score);
            }

            return α;
        }

        private int EvaluateBoard(IBoard board, int depth, int α, int β, int sign)
        {
            if (timeIsUp)
            {
                return 0;
            }

            if (depth == maxDepth)
            {
                return Quiesce(board, α, β, sign);
            }

            var transpositionIndex = board.Hash % transpositionTableSize;
            var cachedEntry = transpositionTable[transpositionIndex];

            if (cachedEntry.Hash == board.Hash && cachedEntry.Depth >= maxDepth - depth)
            {
                switch (cachedEntry.EntryType)
                {
                    case Enum.EntryType.Exact:
                        return cachedEntry.Score;
                    case Enum.EntryType.LowerBound:
                        α = Math.Max(α, cachedEntry.Score);
                        break;
                    case Enum.EntryType.UpperBound:
                        β = Math.Min(β, cachedEntry.Score);
                        break;
                }

                if (α >= β)
                {
                    return cachedEntry.Score;
                }
            }

            var legalmoves = board.PlayLegalMoves();

            if (legalmoves.Count == 0)
            {
                return board.IsChecked ? -Scoring.MateScore : Scoring.DrawScore;
            }

            Move? bestMove = null;
            var α0 = α;
            var currentMoveNumber = 0;

            foreach (var newBoard in SortBoards(legalmoves, cachedEntry.Move))
            {
                nodeCount++;

                if (depth == 0)
                {
                    SendCurrentMove(newBoard, ++currentMoveNumber);
                }

                var evaluation = -EvaluateBoard(newBoard, depth + 1, -β, -α, -sign);

                if (repetitionTable.GetValueOrDefault(newBoard.Hash) >= 2)
                {
                    // A draw be repetition may be forced by either player.
                    // TODO: This does not take into account moves made in the current evaluated line.
                    evaluation = Scoring.DrawScore;
                }

                if (evaluation > α)
                {
                    bestMove = newBoard.Counters.LastMove;
                    α = evaluation;
                }

                if (α >= β)
                {
                    if (newBoard.Counters.Capture == Piece.None)
                    {
                        AddKillerMove(newBoard.Counters.Ply, newBoard.Hash);
                    }
                    break;
                }
            }

            if (transpositionTable[transpositionIndex].Depth <= maxDepth - depth)
            {
                transpositionTable[transpositionIndex] = new TranspositionTableEntry(
                    α <= α0 ? Enum.EntryType.UpperBound : α >= β ? Enum.EntryType.LowerBound : Enum.EntryType.Exact,
                    bestMove!,
                    α,
                    board.Hash,
                    maxDepth - depth
                    );
            }

            if (depth == 0)
            {
                evaluatedBestMove = bestMove;
            }

            return α;
        }

        public void Stop() => timeIsUp = true;
    }
}