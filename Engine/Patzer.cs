using Serilog;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Parsing;

namespace SicTransit.Woodpusher.Engine
{
    public class Patzer : IEngine
    {
        public Board Board { get; private set; }

        private readonly Random random = new();

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

        public void Position(string fen, IReadOnlyCollection<AlgebraicMove> algebraicMoves)
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
                <= 4 => 7,
                <= 12 => 5,
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
            if (depth == 0)
            {
                return board.Score;
            }

            var validMoves = board.GetValidMoves().OrderByDescending(m => m.Target.Flags.HasFlag(SpecialMove.MustTake) || m.Target.Flags.HasFlag(SpecialMove.Promote) || m.Position.Piece.Type != PieceType.Pawn);

            if (board.ActiveColor == PieceColor.White)
            {
                var maxScore = int.MinValue;

                foreach (var move in validMoves)
                {
                    progress.NodeCount++;

                    var score = EvaluateBoard(board.Play(move), depth - 1, progress, alpha, beta);

                    maxScore = Math.Max(maxScore, score);
                    alpha = Math.Max(alpha, score);

                    if (beta <= alpha)
                    {
                        break;
                    }
                }

                return maxScore;
            }
            else
            {
                var minScore = int.MaxValue;

                foreach (var move in validMoves)
                {
                    progress.NodeCount++;

                    var score = EvaluateBoard(board.Play(move), depth - 1, progress, alpha, beta);

                    minScore = Math.Min(minScore, score);
                    beta = Math.Min(beta, score);

                    if (beta <= alpha)
                    {
                        break;
                    }
                }

                return minScore;
            }
        }
    }
}