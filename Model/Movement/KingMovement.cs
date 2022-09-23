using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model.Movement
{
    public static class KingMovement
    {
        private static readonly Square WhiteStartPosition = new("e1");
        private static readonly Square BlackStartPosition = new("e8");

        public static IEnumerable<IEnumerable<Move>> GetTargetVectors(Position position)
        {
            var square = position.Square;
            var r = square.Rank;
            var f = square.File;

            if (r < 7)
            {
                yield return new[] { new Target(square.NewRank(r + 1)).ToMove(position) };

                if (f < 7)
                {
                    yield return new[] { new Target(square.AddFileAndRank(1, 1)).ToMove(position) };
                }
            }

            if (f < 7)
            {
                yield return new[] { new Target(square.NewFile(f + 1)).ToMove(position) };

                if (r > 0)
                {
                    yield return new[] { new Target(square.AddFileAndRank(1, -1)).ToMove(position) };
                }
            }

            if (r > 0)
            {
                yield return new[] { new Target(square.NewRank(r - 1)).ToMove(position) };

                if (f > 0)
                {
                    yield return new[] { new Target(square.AddFileAndRank(-1, -1)).ToMove(position) };
                }
            }

            if (f > 0)
            {
                yield return new[] { new Target(square.NewFile(f - 1)).ToMove(position) };

                if (r < 7)
                {
                    yield return new[] { new Target(square.AddFileAndRank(-1, 1)).ToMove(position) };
                }
            }

            // TODO: There are constants for all those squares!
            if (position.Piece.Color == PieceColor.White)
            {
                if (square.Equals(WhiteStartPosition))
                {
                    yield return new[] { new Target(new Square(6, 0), SpecialMove.CastleKing | SpecialMove.CannotTake, null, new Square(5, 0), new[] { new Square(5, 0), new Square(6, 0) }, new Square(7, 0)).ToMove(position) };
                    yield return new[] { new Target(new Square(2, 0), SpecialMove.CastleQueen | SpecialMove.CannotTake, null, new Square(3, 0), new[] { new Square(2, 0), new Square(1, 0) }, new Square(0, 0)).ToMove(position) };
                }
            }
            else
            {
                if (square.Equals(BlackStartPosition))
                {
                    yield return new[] { new Target(new Square(6, 7), SpecialMove.CastleKing | SpecialMove.CannotTake, null, new Square(5, 7), new[] { new Square(5, 7), new Square(6, 7) }, new Square(7, 7)).ToMove(position) };
                    yield return new[] { new Target(new Square(2, 7), SpecialMove.CastleQueen | SpecialMove.CannotTake, null, new Square(3, 7), new[] { new Square(2, 7), new Square(1, 7) }, new Square(0, 7)).ToMove(position) };
                }
            }
        }
    }

}
