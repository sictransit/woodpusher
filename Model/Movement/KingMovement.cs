using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model.Movement
{
    public static class KingMovement
    {
        private static readonly Square WhiteStartPosition = new("e1");
        private static readonly Square BlackStartPosition = new("e8");

        public static IEnumerable<IEnumerable<Target>> GetTargetVectors(Square square, PieceColor pieceColor)
        {
            var r = square.Rank;
            var f = square.File;

            if (r < 7)
            {
                yield return new[] { new Target(square.NewRank(r + 1)) };

                if (f < 7)
                {
                    yield return new[] { new Target(square.AddFileAndRank(1, 1)) };
                }
            }

            if (f < 7)
            {
                yield return new[] { new Target(square.NewFile(f + 1)) };

                if (r > 0)
                {
                    yield return new[] { new Target(square.AddFileAndRank(1, -1)) };
                }
            }

            if (r > 0)
            {
                yield return new[] { new Target(square.NewRank(r - 1)) };

                if (f > 0)
                {
                    yield return new[] { new Target(square.AddFileAndRank(-1, -1)) };
                }
            }

            if (f > 0)
            {
                yield return new[] { new Target(square.NewFile(f - 1)) };

                if (r < 7)
                {
                    yield return new[] { new Target(square.AddFileAndRank(-1, 1)) };
                }
            }

            if (pieceColor == PieceColor.White)
            {
                if (square.Equals(WhiteStartPosition))
                {
                    yield return new[] { new Target(new Square(6, 0), SpecialMove.CastleKing | SpecialMove.CannotTake, null, new Square(5, 0), null, new Square(7, 0)) };
                    yield return new[] { new Target(new Square(2, 0), SpecialMove.CastleQueen | SpecialMove.CannotTake, null, new Square(3, 0), new Square(1, 0), new Square(0, 0)) };
                }
            }
            else
            {
                if (square.Equals(BlackStartPosition))
                {
                    yield return new[] { new Target(new Square(6, 7), SpecialMove.CastleKing | SpecialMove.CannotTake, null, new Square(5, 7), new Square(7, 7)) };
                    yield return new[] { new Target(new Square(2, 7), SpecialMove.CastleQueen | SpecialMove.CannotTake, null, new Square(3, 7), new Square(1, 7), new Square(0, 7)) };
                }
            }
        }
    }

}
