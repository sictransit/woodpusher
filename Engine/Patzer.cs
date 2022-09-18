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
            Move bestMove = null;

            int bestScore = int.MinValue;

            var sign = Board.ActiveColor == Model.Enums.PieceColor.White ? 1 : -1;

            foreach (var move in Board.GetValidMoves().OrderBy(_ => Guid.NewGuid().ToString()))
            {
                var testBoard = Board.Play(move);

                var score = testBoard.Score * sign;

                if (score > bestScore)
                {
                    bestMove = move;
                    bestScore = score;
                }
            }

            Play(bestMove);

            return new AlgebraicMove(bestMove);
        }

        private static int EvaluateMove(Board board, Move move, int depth, int alpha, int beta)
        {
            var testBoard = board.Play(move);

            if (depth == 0)
            {
                if (testBoard.ActiveColor == PieceColor.White)
                {
                    return Math.Max(alpha, testBoard.Score);
                }
                else
                {
                    return Math.Min(beta, testBoard.Score);
                }
            }
            else
            {
                foreach (var moves in testBoard.GetValidMoves())
                {                    
                    if (testBoard.ActiveColor == PieceColor.White)
                    {
                        return Math.Max(alpha, testBoard.Score);
                    }
                    else
                    {
                        return Math.Min(beta, testBoard.Score);
                    }

                }
            }

        }
    }
}