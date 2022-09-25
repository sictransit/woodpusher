using Serilog;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Model.Interfaces;
using SicTransit.Woodpusher.Parsing;
using System.Diagnostics;

namespace SicTransit.Woodpusher.Engine
{
    public class Patzer : IEngine
    {
        public IBoard Board { get; private set; }

        private readonly Random random = new();

        private const int MATE_SCORE = 1000000;
        private const int WHITE_MATE_SCORE = -MATE_SCORE;
        private const int BLACK_MATE_SCORE = MATE_SCORE;
        private const int DRAW_SCORE = 0;
        private const int MAX_DEPTH = 32;

        private DateTime deadline;

        public Patzer()
        {
            Board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);
        }

        public void Initialize()
        {
            Board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);
        }

        public void Play(Move move)
        {
            Board = Board.PlayMove(move);

            Log.Debug($"played: {move}");
        }

        public void Position(string fen, IEnumerable<AlgebraicMove>? algebraicMoves = null)
        {
            algebraicMoves = algebraicMoves != null ? algebraicMoves : Enumerable.Empty<AlgebraicMove>();

            Board = ForsythEdwardsNotation.Parse(fen);

            foreach (var algebraicMove in algebraicMoves)
            {
                var move = Board.GetLegalMoves().SingleOrDefault(m => m.Position.Square.Equals(algebraicMove.From) && m.Target.Equals(algebraicMove.To) && m.PromotionType == algebraicMove.Promotion);

                if (move == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(algebraicMoves), $"unable to play: {algebraicMove}");
                }

                Play(move);
            }
        }

        public AlgebraicMove FindBestMove(int limit = 1000, Action<string>? infoCallback = null)
        {
            var sw = new Stopwatch();
            sw.Start();

            deadline = DateTime.UtcNow.AddMilliseconds(limit);

            var sign = Board.ActiveColor == PieceColor.White ? 1 : -1;
            var nodeCount = 0ul;

            var evaluations = Board.GetLegalMoves().Select(m => new MoveEvaluation(m)).ToList();

            foreach (var depth in new[] { 1, 2, 3, 5, 8, 13, 21 })
            {
                if (evaluations.Count() <= 1)
                {
                    break;
                }

                if (evaluations.Any(e => Math.Abs(Math.Abs(e.Score) - MATE_SCORE) < MAX_DEPTH))
                {
                    break;
                }

                // 2,1 Mnode/s
                Parallel.ForEach(evaluations.OrderByDescending(e => e.Score).ToArray(), e =>
                {
                    if (DateTime.UtcNow > deadline)
                    {
                        Log.Debug("aborting due to lack of time");

                        return;
                    }

                    var board = Board.PlayMove(e.Move);

                    e.Score = EvaluateBoard(board, board.ActiveColor == PieceColor.White, depth, e) * sign;

                    nodeCount += e.NodeCount;

                    infoCallback?.Invoke($"info depth {depth} nodes {nodeCount} score cp {e.Score * sign} pv {e.Move.ToAlgebraicMoveNotation()} nps {nodeCount * 1000 / (ulong)(1 + sw.ElapsedMilliseconds)}");
                });
            }

            if (evaluations.Any())
            {
                var orderedEvaluations = evaluations.GroupBy(e => e.Score).OrderByDescending(g => g.Key).ToArray();
                var bestEvaluations = orderedEvaluations.First().ToArray();

                var evaluation = bestEvaluations[random.Next(bestEvaluations.Length)];

                Log.Information($"evaluated {nodeCount} nodes, found: {evaluation.Move} {evaluation.Score * sign}");

                return new AlgebraicMove(evaluation.Move);
            }

            Log.Warning("no legal moves found");

            return null;
        }

        private int EvaluateBoard(IBoard board, bool maximizing, int depth, MoveEvaluation evaluation, int alpha = int.MinValue, int beta = int.MaxValue)
        {
            var moves = board.GetLegalMoves().ToArray();

            if (moves.Length == 0)
            {
                return board.IsChecked ? maximizing ? WHITE_MATE_SCORE - depth : BLACK_MATE_SCORE + depth : DRAW_SCORE;
            }

            if (depth == 0 || DateTime.UtcNow > deadline)
            {
                return board.Score;
            }

            var bestScore = maximizing ? int.MinValue : int.MaxValue;

            foreach (var move in moves)
            {
                evaluation.NodeCount++;

                var score = EvaluateBoard(board.PlayMove(move), !maximizing, depth - 1, evaluation, alpha, beta);

                if (maximizing)
                {
                    bestScore = Math.Max(bestScore, score);

                    if (bestScore >= beta)
                    {
                        break;
                    }

                    alpha = Math.Max(alpha, bestScore);
                }
                else
                {
                    bestScore = Math.Min(bestScore, score);

                    if (bestScore <= alpha)
                    {
                        break;
                    }

                    beta = Math.Min(beta, bestScore);
                }
            }

            return bestScore;
        }

        public void Stop()
        {
            deadline = DateTime.UtcNow;
        }
    }
}