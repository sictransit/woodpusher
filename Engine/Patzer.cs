using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Common.Extensions;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Lookup;
using SicTransit.Woodpusher.Common.Parsing;
using SicTransit.Woodpusher.Engine.Enum;
using SicTransit.Woodpusher.Engine.Extensions;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Diagnostics;
using System.Text;

namespace SicTransit.Woodpusher.Engine
{
    public class Patzer : IEngine
    {
        public Board Board { get; private set; }

        private volatile bool timeIsUp = false;
        private int selDepth = 0;
        private uint nodeCount = 0;
        private const uint EngineMaxDepth = 128;

        private OpeningBook? openingBook;

        private readonly Action<string, bool>? infoCallback;

        private const int transpositionTableSize = 1_000_000;

        private readonly TranspositionTableEntry[] transpositionTable = new TranspositionTableEntry[transpositionTableSize];
        private readonly Dictionary<ulong, int> repetitionTable = [];
        private readonly ulong[][] killerMoves = new ulong[1000][]; // TODO: Phase out killer moves as the game progresses.
        private readonly int[,] historyHeuristics = new int[64, 64];

        private readonly List<Move> bestLine = [];

        private EngineOptions engineOptions;

        public Patzer(Action<string, bool>? infoCallback = null)
        {
            this.infoCallback = infoCallback;

            Initialize(EngineOptions.Default);
        }

        public void Initialize(EngineOptions options)
        {
            engineOptions = options;

            SetBoard(Board.StartingPosition());

            openingBook = null;

            for (var i = 0; i < killerMoves.Length; i++)
            {
                killerMoves[i] = new ulong[2];
            }
        }

        private void SetBoard(Board board)
        {
            Board = board;

            repetitionTable.Clear();
            repetitionTable[Board.Hash] = 1;
        }

        private void SendCallbackInfo(string message, bool info) => infoCallback?.Invoke(message, info);

        public void Play(Move move)
        {
            Log.Debug("{Color} plays: {Move}", Board.ActiveColor.Is(Piece.White) ? "White" : "Black", move);

            Board = Board.Play(move);

            if (Board.Counters.HalfmoveClock == 0)
            {
                repetitionTable.Clear();
            }

            repetitionTable[Board.Hash] = repetitionTable.GetValueOrDefault(Board.Hash) + 1;
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
            ulong totalNodes = 0;
            var sb = new StringBuilder();

            foreach (var board in Board.PlayLegalMoves())
            {
                var nodes = depth > 1 ? board.Perft(depth) : 1;
                sb.AppendLine($"{board.Counters.LastMove.ToAlgebraicMoveNotation()}: {nodes}");

                totalNodes += nodes;
            }
            SendCallbackInfo(sb.ToString() + Environment.NewLine + $"Nodes searched: {totalNodes}", false);
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

        public AlgebraicMove? GetPonderMove() 
        { 
            var ponderMove = bestLine.Count < 2 ? null : bestLine[1]; 

            return ponderMove == null ? null : new AlgebraicMove(ponderMove);
        }

        private void AddKillerMove(int ply, ulong hash)
        {
            killerMoves[ply][1] = killerMoves[ply][0];
            killerMoves[ply][0] = hash;
        }

        private Move? GetBookMove()
        {
            openingBook ??= Board.ActiveColor == Piece.White ? new OpeningBook(Piece.White) : new OpeningBook(Piece.None);

            return openingBook.GetMove(Board) ?? openingBook.GetTheoryMove(Board);
        }

        private AlgebraicMove? SearchForBestMove(Stopwatch stopwatch, int timeLimit = 1000)
        {
            var depth = 0;
            nodeCount = 0;
            Move? bestMove = null;
            var enoughTime = true;
            var progress = new List<(int depth, long time)>();
            Array.Clear(transpositionTable, 0, transpositionTable.Length);
            Array.Clear(historyHeuristics, 0, historyHeuristics.Length);

            if (engineOptions.UseOpeningBook)
            {
                var bookMove = GetBookMove();
                if (bookMove != null)
                {
                    UpdateBestLine(bookMove, 1);
                    Log.Information("Returning book move: {0}", bookMove);
                    SendDebugInfo($"playing book move {bookMove.ToAlgebraicMoveNotation()}");
                    SendProgress(stopwatch, 1, 1, Board.Play(bookMove).Score, null);
                    return new AlgebraicMove(bookMove);
                }
            }

            while (depth < EngineMaxDepth)
            {
                depth++;
                selDepth = depth;
                long startTime = stopwatch.ElapsedMilliseconds;
                int? mateIn = default;

                var score = EvaluateBoard(Board, depth, -Scoring.MoveMaximumScore, Scoring.MoveMaximumScore, Board.ActiveColor.Is(Piece.White) ? 1 : -1, true);

                long evaluationTime = stopwatch.ElapsedMilliseconds - startTime;

                if (!timeIsUp || (bestMove == null))
                {
                    var ttEntry = transpositionTable[Board.Hash % transpositionTableSize];

                    if (ttEntry.Hash == Board.Hash && ttEntry.EntryType == EntryType.Exact)
                    {
                        bestMove = ttEntry.Move;
                    }
                    else
                    {
                        throw new InvalidOperationException("Unable to find best move in transposition table.");
                    }

                    if (bestMove != null)
                    {
                        UpdateBestLine(bestMove, depth);
                    }

                    var mateScore = Scoring.MateScore - Math.Abs(score) - Board.Counters.Ply + 1;

                    if (mateScore <= EngineMaxDepth)
                    {
                        mateIn = mateScore / 2 * Math.Sign(score);
                    }

                    SendProgress(stopwatch, depth, nodeCount, score, mateIn);

                    if (evaluationTime > 0)
                    {
                        progress.Add((depth, evaluationTime));

                        if (progress.Count > 2)
                        {
                            var estimatedTime = MathExtensions.ApproximateNextDepthTime(progress, depth + 1);
                            var remainingTime = timeLimit - stopwatch.ElapsedMilliseconds;
                            enoughTime = remainingTime > estimatedTime;
                            Log.Debug("Estimated time for next depth: {0}ms, remaining time: {1}ms, enough time: {2}", estimatedTime, remainingTime, enoughTime);
                        }
                    }
                }

                string? abortMessage = null;

                if (mateIn.HasValue)
                {
                    abortMessage = $"aborting search @ depth {depth}, mate in {mateIn}";
                }
                else if (timeIsUp)
                {
                    abortMessage = $"aborting search @ depth {depth}, time is up";
                }
                else if (!enoughTime)
                {
                    abortMessage = $"aborting search @ depth {depth}, not enough time";
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

        private void SendProgress(Stopwatch stopwatch, int depth, uint nodes, int score, int? mateIn)
        {
            var nodesPerSecond = stopwatch.ElapsedMilliseconds == 0 ? 0 : nodes * 1000 / stopwatch.ElapsedMilliseconds;
            var hashFull = transpositionTable.Count(t => t.Hash != 0) * 1000 / transpositionTableSize;
            var scoreString = mateIn.HasValue ? $"mate {mateIn.Value}" : $"cp {score}";
            var pvString = string.Join(' ', bestLine.Select(m => m.ToAlgebraicMoveNotation()));

            SendInfo($"depth {depth} seldepth {selDepth} nodes {nodes} nps {nodesPerSecond} hashfull {hashFull} score {scoreString} time {stopwatch.ElapsedMilliseconds} pv {pvString}");
        }

        private void SendCurrentMove(Move move, int depth, int currentMoveNumber)
        {
            SendInfo($"depth {depth} currmove {move.ToAlgebraicMoveNotation()} currmovenumber {currentMoveNumber}");
        }

        private void UpdateBestLine(Move bestMove, int depth)
        {
            bestLine.Clear();
            bestLine.Add(bestMove);

            var board = Board.Play(bestMove);

            for (var i = 0; i < depth; i++)
            {
                var entry = transpositionTable[board.Hash % transpositionTableSize];
                if (entry.EntryType == EntryType.Exact && entry.Hash == board.Hash && board.GetLegalMoves().Contains(entry.Move))
                {
                    bestLine.Add(entry.Move);
                    board = board.Play(entry.Move);
                }
                else
                {
                    break;
                }
            }
        }

        private void SendInfo(string info) => SendCallbackInfo($"info {info}", true);

        private void SendDebugInfo(string debugInfo) => SendInfo($"string debug {debugInfo}");

        private void SendExceptionInfo(Exception exception) => SendInfo($"string exception {exception.GetType().Name} {exception.Message}");

        private IEnumerable<Board> SortBoards(IEnumerable<Board> boards, Move? preferredMove = null)
        {
            return boards.OrderByDescending(board =>
            {
                var move = board.Counters.LastMove;

                if (preferredMove != null && move.Equals(preferredMove))
                {
                    return int.MaxValue; // Highest priority for preferred move.
                }

                if (transpositionTable[board.Hash % transpositionTableSize].EntryType == EntryType.Exact)
                {
                    return int.MaxValue - 10; // High priority for exact transposition table entries.
                }

                if (board.Counters.Capture != Piece.None)
                {
                    return int.MaxValue - 20 + Scoring.GetBasicPieceValue(board.Counters.Capture) - Scoring.GetBasicPieceValue(board.Counters.LastMove.Piece); // Capture value, sorting valuable captures first.
                }

                if (killerMoves[board.Counters.Ply].Contains(board.Hash))
                {
                    return int.MaxValue - 30; // High priority for killer moves
                }

                if (board.IsChecked)
                {
                    return int.MaxValue - 40;
                }

                return int.MinValue + historyHeuristics[move.FromIndex, move.ToIndex];
            });
        }

        private int Quiesce(Board board, int α, int β, int sign)
        {
            if (timeIsUp)
            {
                return 0;
            }

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

        private int EvaluateBoard(Board board, int depth, int α, int β, int sign, bool isRootNode = false)
        {
            if (timeIsUp)
            {
                return 0;
            }

            if (depth == 0)
            {
                return Quiesce(board, α, β, sign);
            }

            var transpositionIndex = board.Hash % transpositionTableSize;
            var cachedEntry = transpositionTable[transpositionIndex];

            if (cachedEntry.Hash == board.Hash && cachedEntry.Depth >= depth)
            {
                switch (cachedEntry.EntryType)
                {
                    case EntryType.Exact:
                        return cachedEntry.Score;
                    case EntryType.LowerBound:
                        α = Math.Max(α, cachedEntry.Score);
                        break;
                    case EntryType.UpperBound:
                        β = Math.Min(β, cachedEntry.Score);
                        break;
                }

                if (α >= β)
                {
                    return cachedEntry.Score;
                }
            }

            Move? bestMove = null;
            var α0 = α;
            var n0 = nodeCount;
            var currentMoveNumber = 0;            

            foreach (var newBoard in SortBoards(board.PlayLegalMoves(), cachedEntry.Move))
            {
                nodeCount++;

                if (isRootNode)
                {
                    SendCurrentMove(newBoard.Counters.LastMove, depth, ++currentMoveNumber);
                }

                int evaluation;

                if (repetitionTable.GetValueOrDefault(newBoard.Hash) >= 2)
                {
                    // A draw by repetition may be forced by either player.
                    // TODO: This does not take into account moves made in the current evaluated line.
                    evaluation = Scoring.DrawScore;
                }
                else
                {
                    if (nodeCount == n0 + 1)
                    {
                        evaluation = -EvaluateBoard(newBoard, depth - 1, -β, -α, -sign);
                    }
                    else
                    {
                        evaluation = -EvaluateBoard(newBoard, depth - 1, -α - 1, -α, -sign);
                        if (α < evaluation && evaluation < β)
                        {
                            evaluation = -EvaluateBoard(newBoard, depth - 1, -β, -α, -sign);
                        }
                    }
                }

                if (evaluation > α)
                {
                    bestMove = newBoard.Counters.LastMove;
                    historyHeuristics[bestMove.FromIndex, bestMove.ToIndex] += depth * depth;
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

            if (n0 == nodeCount)
            {
                return board.IsChecked ? -Scoring.MateScore + board.Counters.Ply : Scoring.DrawScore;
            }

            var ttEntry = transpositionTable[transpositionIndex];

            if (ttEntry.EntryType == EntryType.None || ttEntry.Depth <= depth)
            {
                transpositionTable[transpositionIndex] = new TranspositionTableEntry(
                    α <= α0 ? EntryType.UpperBound : α >= β ? EntryType.LowerBound : EntryType.Exact,
                    bestMove!,
                    α,
                    board.Hash,
                    depth
                    );
            }

            return α;
        }

        public void Stop() => timeIsUp = true;
    }
}