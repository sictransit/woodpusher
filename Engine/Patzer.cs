using Serilog;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Parsing;
using System.Diagnostics;

namespace SicTransit.Woodpusher.Engine
{
    public class Patzer : IEngine
    {
        public Board Board { get; private set; }

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
            Board = Board.Play(move);

            Log.Debug($"played: {move}");
        }

        public void Position(string fen, IEnumerable<AlgebraicMove> algebraicMoves)
        {
            Board = ForsythEdwardsNotation.Parse(fen);

            foreach (var algebraicMove in algebraicMoves)
            {
                var move = Board.GetValidMoves().SingleOrDefault(m => m.Position.Square.Equals(algebraicMove.From) && m.Target.Square.Equals(algebraicMove.To) && m.Target.PromotionType == algebraicMove.Promotion);

                if (move == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(algebraicMoves), $"unable to play: {algebraicMove}");
                }

                Play(move);
            }
        }

        public AlgebraicMove FindBestMove(int limit = 1000, Action<string> infoCallback = null)
        {
            var sw = new Stopwatch();
            sw.Start();

            deadline = DateTime.UtcNow.AddMilliseconds(limit);

            var sign = Board.ActiveColor == PieceColor.White ? 1 : -1;
            var nodeCount = 0;

            var evaluations = Board.GetValidMoves().Select(m => new MoveEvaluation { Move = m }).ToList();

            foreach (var depth in new[] { 1, 3, 5, 8, 13, 21 })
            {
                if (evaluations.Count() <= 1)
                {
                    break;
                }

                if (evaluations.Any(e => Math.Abs(Math.Abs(e.Score) - MATE_SCORE) < MAX_DEPTH))
                {
                    break;
                }

                foreach (var evaluation in evaluations.OrderByDescending(e => e.Score).ToArray())
                {
                    if (DateTime.UtcNow > deadline)
                    {
                        Log.Debug("aborting due to lack of time");

                        break;
                    }

                    var board = Board.Play(evaluation.Move);

                    evaluation.Score = EvaluateBoard(board, board.ActiveColor == PieceColor.White, depth, evaluation) * sign;

                    nodeCount += evaluation.NodeCount;

                    infoCallback?.Invoke($"info depth {depth} nodes {nodeCount} score cp {evaluation.Score * sign} pv {evaluation.Move.ToAlgebraicMoveNotation()} nps {nodeCount * 1000 / sw.ElapsedMilliseconds}");
                }
            }

            if (evaluations.Any())
            {
                var orderedEvaluations = evaluations.GroupBy(e => e.Score).OrderByDescending(g => g.Key).ToArray();
                var bestEvaluations = orderedEvaluations.First().ToArray();

                var evaluation = bestEvaluations[random.Next(bestEvaluations.Length)];

                Log.Information($"evaluated {nodeCount} nodes, found: {evaluation.Move} {evaluation.Score * sign}");

                return new AlgebraicMove(evaluation.Move);
            }

            Log.Warning("no valid moves found");

            return null;
        }

        private int EvaluateBoard(Board board, bool maximizing, int depth, MoveEvaluation evaluation, int alpha = int.MinValue, int beta = int.MaxValue)
        {
            var validMoves = board.GetValidMoves().ToArray();            

            if (validMoves.Length == 0)
            {
                return board.IsChecked ? maximizing ? WHITE_MATE_SCORE - depth : BLACK_MATE_SCORE + depth : DRAW_SCORE;
            }

            if (depth == 0)
            {
                return board.Score;
            }

            var bestScore = maximizing ? int.MinValue : int.MaxValue;

            foreach (var move in validMoves)
            {
                evaluation.NodeCount++;

                var score = EvaluateBoard(board.Play(move), !maximizing, depth--, evaluation, alpha, beta);

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