using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Engine.Movement
{
    public static class KingMovement
    {
        private static Square WhiteStartPosition = Square.FromAlgebraicNotation("e1");
        private static Square BlackStartPosition = Square.FromAlgebraicNotation("e8");

        public static IEnumerable<IEnumerable<Move>> GetTargetVectors(Square square, PieceColour pieceColour)
        {
            var r = square.Rank;
            var f = square.File;

            if (r < 7)
            {
                yield return new[] { new Move(square.NewRank(r + 1)) };

                if (f < 7)
                {
                    yield return new[] { new Move(square.AddFileAndRank(1, 1)) };
                }
            }

            if (f < 7)
            {
                yield return new[] { new Move(square.NewFile(f + 1)) };

                if (r > 0)
                {
                    yield return new[] { new Move(square.AddFileAndRank(1, -1)) };
                }
            }

            if (r > 0)
            {
                yield return new[] { new Move(square.NewRank(r - 1)) };

                if (f > 0)
                {
                    yield return new[] { new Move(square.AddFileAndRank(-1, -1)) };
                }
            }

            if (f > 0)
            {
                yield return new[] { new Move(square.NewFile(f - 1)) };

                if (r < 7)
                {
                    yield return new[] { new Move(square.AddFileAndRank(-1, 1)) };
                }
            }

            if (pieceColour == PieceColour.White)
            {
                if (square.Equals(WhiteStartPosition))
                {
                    yield return new[] { new Move(new Square(6, 0), MovementFlags.CastleKing) };
                    yield return new[] { new Move(new Square(2, 0), MovementFlags.CastleQueen) };
                }
            }
            else
            {
                if (square.Equals(BlackStartPosition))
                {
                    yield return new[] { new Move(new Square(6, 7), MovementFlags.CastleKing) };
                    yield return new[] { new Move(new Square(2, 7), MovementFlags.CastleQueen) };
                }
            }
        }
    }

}
