﻿using Serilog;
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

        // TODO: This doesn't stop opponent from forcing a draw.
        //private readonly Dictionary<ulong, int> repetitions = new();

        private readonly OpeningBook whiteOpeningBook;
        private readonly OpeningBook blackOpeningBook;

        private readonly Action<string>? infoCallback;

        private const int transpositionTableSize = 1_000_000;

        private readonly TranspositionTableEntry[] transpositionTable = new TranspositionTableEntry[transpositionTableSize];

        private readonly List<(int ply, Move move)> bestLine = new();

        private Move? evaluatedBestMove = null;

        public Patzer(Action<string>? infoCallback = null)
        {
            whiteOpeningBook = new OpeningBook(Piece.White);
            blackOpeningBook = new OpeningBook(Piece.None);
            Board = Common.Board.StartingPosition();
            this.infoCallback = infoCallback;
        }

        public void Initialize()
        {
            Board = Common.Board.StartingPosition();
            //repetitions.Clear();
            //repetitions[Board.Hash] = 1;
        }

        private void SendCallbackInfo(string info) => infoCallback?.Invoke(info);

        public void Play(Move move)
        {
            var color = Board.ActiveColor.Is(Piece.White) ? "White" : "Black";
            Log.Debug("{Color} plays: {Move}", color, move);
            Board = Board.Play(move);
            //repetitions[Board.Hash] = repetitions.GetValueOrDefault(Board.Hash) + 1;
        }

        private Move? GetOpeningBookMove()
        {
            var openingBook = Board.ActiveColor == Piece.White ? whiteOpeningBook : blackOpeningBook;
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
            var openingBook = Board.ActiveColor == Piece.White ? whiteOpeningBook : blackOpeningBook;
            var theoryMoves = Board.PlayLegalMoves()
                .Select(b => new { move = b.Counters.LastMove, count = openingBook?.GetMoves(b.Hash).Count() ?? 0 })
                .Where(t => t.count > 0)
                .ToArray();
            return theoryMoves.OrderByDescending(m => m.count).FirstOrDefault()?.move;
        }

        public void Position(string fen, IEnumerable<AlgebraicMove>? algebraicMoves = null)
        {
            algebraicMoves ??= Array.Empty<AlgebraicMove>();
            Board = ForsythEdwardsNotation.Parse(fen);
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
            maxDepth = 0;
            nodeCount = 0;
            timeIsUp = false;
            
            stopwatch.Restart();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                Log.Debug("thinking time: {TimeLimit}", timeLimit);
                Thread.Sleep(timeLimit);
                timeIsUp = true;
            });            

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
            Move? bestMove = null;
            var foundMate = false;
            var enoughTime = true;
            var progress = new List<(int depth, long time)>();

            Array.Clear(transpositionTable, 0, transpositionTable.Length);        

            while (maxDepth < Declarations.MaxDepth && !foundMate && !timeIsUp && enoughTime)
            {
                try
                {
                    maxDepth++;
                    long startTime = stopwatch.ElapsedMilliseconds;
                    var score = EvaluateBoard(Board, 0, -Declarations.MoveMaximumScore, Declarations.MoveMaximumScore, sign);
                    long evaluationTime = stopwatch.ElapsedMilliseconds - startTime;

                    if (!timeIsUp || (bestMove == null))
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

            return bestMove == null ? null : new AlgebraicMove(bestMove);
        }

        private void UpdateBestLine(Move bestMove, int depth)
        {
            var ply = Board.Counters.Ply + 1;
            bestLine.Add((ply, bestMove));
            var board = Board.Play(bestMove);

            for (var i = 0; i < depth; i++)
            {
                var entry = transpositionTable[board.Hash % transpositionTableSize];
                if (entry.EntryType == Enum.EntryType.Exact && entry.Hash == board.Hash && entry.Move != null && board.GetLegalMoves().Contains(entry.Move))
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

        private int? CalculateMateIn(int evaluation, int playerSign)
        {
            var mateInPlies = Math.Abs(Math.Abs(evaluation) - Declarations.MateScore);
            if (mateInPlies <= Declarations.MaxDepth)
            {
                var resultSign = Math.Sign(evaluation);

                return (mateInPlies / 2 + playerSign) * resultSign;
            }
            return null;
        }

        private void SendInfo(string info) => SendCallbackInfo($"info {info}");

        private void SendDebugInfo(string debugInfo) => SendInfo($"string debug {debugInfo}");

        private void SendExceptionInfo(Exception exception) => SendInfo($"string exception {exception.GetType().Name} {exception.Message}");

        private int Quiesce(IBoard board, int α, int β, int sign)
        {
            var standPat = board.Score * sign;
            
            if (standPat >= β)
            {
                return β;
            }

            α = Math.Max(α, standPat);

            var boards = board.PlayLegalMoves(true).OrderByDescending(b => b.Score * sign);
            foreach (var newBoard in boards)
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

            var α0 = α;

            if (depth == maxDepth)
            {
                return Quiesce(board, α, β,  sign);
            }

            var transpositionIndex = board.Hash % transpositionTableSize;
            var cachedEntry = transpositionTable[transpositionIndex];

            if (cachedEntry.EntryType != Enum.EntryType.None && cachedEntry.Hash == board.Hash && cachedEntry.Ply >= maxDepth- depth )
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

            var boards = board.PlayLegalMoves().OrderByDescending(b => b.Counters.LastMove.Equals(cachedEntry.Move)).ThenByDescending(b => b.Score * sign);            

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

            if (bestMove == null)
            {
                bestScore = board.IsChecked ? -Declarations.MateScore + depth : Declarations.DrawScore;
            }

            var entryType = bestScore <= α0 ? Enum.EntryType.UpperBound : bestScore >= β ? Enum.EntryType.LowerBound : Enum.EntryType.Exact;

            transpositionTable[transpositionIndex] = new TranspositionTableEntry(entryType, bestMove, bestScore, board.Hash, maxDepth-depth);
            evaluatedBestMove = bestMove;
            return bestScore;
        }

        public void Stop() => timeIsUp = true;
    }
}