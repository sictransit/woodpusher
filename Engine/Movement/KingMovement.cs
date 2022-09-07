using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Engine.Movement
{
    public static class KingMovement
    {
        private static Square WhiteStartPosition = new("e1");
        private static Square BlackStartPosition = new("e8");

        public static IEnumerable<IEnumerable<Target>> GetTargetVectors(Square square, PieceColour pieceColour)
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

            if (pieceColour == PieceColour.White)
            {
                if (square.Equals(WhiteStartPosition))
                {
                    yield return new[] { new Target(new Square(6, 0), SpecialMove.CastleKing, new Square(5, 0)) };
                    yield return new[] { new Target(new Square(2, 0), SpecialMove.CastleQueen, new Square(3, 0)) };
                }
            }
            else
            {
                if (square.Equals(BlackStartPosition))
                {
                    yield return new[] { new Target(new Square(6, 7), SpecialMove.CastleKing, new Square(5, 7)) };
                    yield return new[] { new Target(new Square(2, 7), SpecialMove.CastleQueen, new Square(3, 7)) };
                }
            }
        }
    }

}
