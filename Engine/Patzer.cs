using Serilog;
using SicTransit.Woodpusher.Common.Extensions;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Lookup;
using SicTransit.Woodpusher.Common.Parsing;
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

        private readonly HashSet<ulong> cutoffs = new();

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
                return new BestMove(new AlgebraicMove(openingMove));
            }

            var sign = Board.ActiveColor.Is(Piece.White) ? 1 : -1;

            var bestMove = default(Move);            

            var foundMate = false;            

            while (maxDepth < Declarations.MaxDepth - 2 && !foundMate && !timeIsUp)
            {
                try
                {
                    maxDepth += 2;

                    transpositionTable.Clear();
                    cutoffs.Clear();

                    // TODO: Check for threefold repetition. Note that we might seek that!

                    var (move, score) = EvaluateBoard(Board, 0, -Declarations.MoveMaximumScore, Declarations.MoveMaximumScore, Board.ActiveColor.Is(Piece.White));

                    if (!timeIsUp)
                    {
                        bestMove = move!;

                        var nodesPerSecond = stopwatch.ElapsedMilliseconds == 0 ? 0 : nodeCount * 1000 / stopwatch.ElapsedMilliseconds;
                        var mateIn = CalculateMateIn(score, sign);
                        foundMate = mateIn is > 0;

                        var scoreString = mateIn.HasValue ? $"mate {mateIn.Value}" : $"cp {score * sign}";

                        var pvString = move != null ? GetPrincipalVariationString(move, maxDepth) : string.Empty;
                        SendInfo($"depth {maxDepth} nodes {nodeCount} nps {nodesPerSecond} score {scoreString} time {stopwatch.ElapsedMilliseconds} {pvString}");
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

            // TODO! Return ponder move.

            return new BestMove(new AlgebraicMove(bestMove));
        }

        private string GetPrincipalVariationString(Move move, int depth)
        {
            var principalVariation = new List<string>
            {
                "pv",
                move.ToAlgebraicMoveNotation()
            };

            var pvBoard = Board.Play(move);

            for (var i = 0; i < depth; i++)
            {
                if (transpositionTable.TryGetValue(pvBoard.Hash, out var cached))
                {
                    principalVariation.Add(cached.move.ToAlgebraicMoveNotation());

                    pvBoard = pvBoard.Play(cached.move);
                }
                else
                {
                    break;
                }
            }

            return string.Join(" ", principalVariation);
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
            //if (transpositionTable.TryGetValue(board.Hash, out var cached) && cached.ply >= board.Counters.Ply)
            //{
            //    //Log.Debug($"Transposition table hit: {board.Hash} {cached.move} {cached.score}");

            //    return (cached.move, cached.score);
            //}

            Move? bestMove = default;

            var bestScore =  -Declarations.MoveMaximumScore ;

            var legalMoves = board.GetLegalMoves();
            //var legalMoves = board.GetLegalMoves().OrderByDescending(l => transpositionTable.ContainsKey(l.Board.Hash));

            foreach (var legalMove in legalMoves)
            {
                nodeCount++;

                var (_, score) = EvaluateBoard(legalMove.Board, depth + 1, -β, -α, !maximizing);
                score=-score;

                if (score > bestScore)
                {
                    bestMove = legalMove.Move;
                    bestScore = score;
                }

                α = Math.Max(α, bestScore);

                if (α >= β)
                {
                    cutoffs.Add(legalMove.Board.Hash);
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