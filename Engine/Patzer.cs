using Serilog;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
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

        public void Play(string algebraic)
        {
            if (string.IsNullOrEmpty(algebraic))
            {
                throw new ArgumentException($"'{nameof(algebraic)}' cannot be null or != 4 chars.", nameof(algebraic));
            }

            var fromSquare = new Square(algebraic[..2]);
            var toSquare = new Square(algebraic[2..]);

            var move = Board.GetValidMoves().SingleOrDefault(m => m.Position.Square.Equals(fromSquare) && m.Target.Square.Equals(toSquare));

            if (move == null)
            {
                throw new ArgumentOutOfRangeException(nameof(algebraic), $"unable to play: {algebraic}");
            }

            Play(move);
        }

        public Move PlayBestMove()
        {
            var moves = Board.GetValidMoves().ToArray();

            var move = moves[random.Next(moves.Length)];

            Play(move);

            return move;
        }
    }
}