using Serilog;
using Serilog.Events;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Parsing;
using static System.Formats.Asn1.AsnWriter;
using static System.String;

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

        public AlgebraicMove FindBestMove()
        {
            var bestMoves = new List<Move>();

            var sign = Board.ActiveColor == PieceColor.White ? 1 : -1;
            var bestScore = int.MinValue;
            var nodeCount = 0;

            var maxDepth = Board.PieceCount switch
            {
                <= 8 => 5,
                _ => 3
            };

            foreach (var move in Board.GetValidMoves())
            {
                var progress = new EvaluationProgress();

                var score = EvaluateBoard(Board.Play(move), maxDepth, progress) * sign;

                if (score > bestScore)
                {
                    bestMoves = new List<Move> { move };
                    bestScore = score;
                }
                else if (score == bestScore)
                {
                    bestMoves.Add(move);
                }

                nodeCount += progress.NodeCount;
            }

            if (bestMoves.Any())
            {
                var bestMove = bestMoves[random.Next(bestMoves.Count)];

                Log.Information($"evaluated {nodeCount} nodes, found: {bestMove} {bestScore * sign}");

                return new AlgebraicMove(bestMove);
            }

            Log.Warning("no valid moves found");

            return null;
        }

        private static int EvaluateBoard(Board board, int depth, EvaluationProgress progress, int alpha = int.MinValue, int beta = int.MaxValue)
        {
            var validMoves = board.GetValidMoves().ToArray();
            var maximizing = board.ActiveColor == PieceColor.White;

            if (!validMoves.Any())
            {
                if (board.IsChecked)
                {
                    return maximizing ? WHITE_MATE_SCORE-depth : BLACK_MATE_SCORE+depth;
                }

                return DRAW_SCORE;
            }

            if (depth == 0 )
            {
                return board.Score;
            }

            var bestScore = maximizing ? int.MinValue : int.MaxValue;

            foreach (var move in validMoves)
            {
                progress.NodeCount++;

                var score = EvaluateBoard(board.Play(move), depth - 1, progress, alpha, beta);

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
    }
}