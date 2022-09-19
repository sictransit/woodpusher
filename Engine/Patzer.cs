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

        private readonly Stack<Board> game;

        private readonly Random random = new();

        public Patzer()
        {
            game = new Stack<Board>();

            Board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);
        }

        public void Initialize(string fen)
        {
            game.Clear();

            Board = ForsythEdwardsNotation.Parse(fen);
        }

        public void Play(Move move)
        {
            game.Push(Board.Copy(Board));

            Board = Board.Play(move);

            Log.Information($"played: {move}");
        }

        public void Play(AlgebraicMove algebraicMove)
        {
            var move = Board.GetValidMoves().SingleOrDefault(m => m.Position.Square.Equals(algebraicMove.From) && m.Target.Square.Equals(algebraicMove.To) && m.Target.PromotionType == algebraicMove.Promotion);

            if (move == null)
            {
                throw new ArgumentOutOfRangeException(nameof(algebraicMove), $"unable to play: {algebraicMove}");
            }

            Play(move);
        }

        public AlgebraicMove PlayBestMove()
        {
            var bestMoves = new List<Move>();

            var sign = Board.ActiveColor == PieceColor.White ? 1 : -1;
            var bestScore = int.MinValue;

            foreach (var move in Board.GetValidMoves().ToArray())
            {
                var score = EvaluateBoard(Board.Play(move), 3) * sign;

                if (score > bestScore)
                {
                    bestMoves = new List<Move> { move };
                    bestScore = score;
                }
                else if (score == bestScore)
                {
                    bestMoves.Add(move);
                }
            }

            if (bestMoves.Any())
            {
                var bestMove = bestMoves[random.Next(bestMoves.Count)];

                Log.Information($"found: {bestMove} {bestScore * sign}");

                Play(bestMove);

                return new AlgebraicMove(bestMove);
            }

            Log.Warning("no valid moves found");

            return default;
        }

        private int EvaluateBoard(Board board, int depth)
        {
            if (depth == 0)
            {
                return board.Score;
            }

            bool maximizing = board.ActiveColor == PieceColor.White;

            if (maximizing)
            {
                var maxScore = int.MinValue;

                foreach (var move in board.GetValidMoves())
                {
                    var testBoard = board.Play(move);
                    maxScore = Math.Max(maxScore, EvaluateBoard(testBoard, depth - 1));
                }
                return maxScore;
            }
            else
            {
                var minScore = int.MaxValue;

                foreach (var move in board.GetValidMoves())
                {
                    var testBoard = board.Play(move);
                    minScore = Math.Min(minScore, EvaluateBoard(testBoard, depth - 1));
                }
                return minScore;
            }
        }
    }
}