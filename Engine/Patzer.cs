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
            var moves = Board.GetValidMoves().ToArray();

            var move = moves[random.Next(moves.Length)];

            Play(move);

            return new AlgebraicMove(move);
        }
    }
}