using SicTransit.Woodpusher.Model.Enums;

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
                yield return new[] { new Move(position, square.NewRank(r + 1)) };

                if (f < 7)
                {
                    yield return new[] { new Move(position, square.AddFileAndRank(1, 1)) };
                }
            }

            if (f < 7)
            {
                yield return new[] { new Move(position, square.NewFile(f + 1)) };

                if (r > 0)
                {
                    yield return new[] { new Move(position, square.AddFileAndRank(1, -1)) };
                }
            }

            if (r > 0)
            {
                yield return new[] { new Move(position, square.NewRank(r - 1)) };

                if (f > 0)
                {
                    yield return new[] { new Move(position, square.AddFileAndRank(-1, -1)) };
                }
            }

            if (f > 0)
            {
                yield return new[] { new Move(position, square.NewFile(f - 1)) };

                if (r < 7)
                {
                    yield return new[] { new Move(position, square.AddFileAndRank(-1, 1)) };
                }
            }

            // TODO: There are constants for all those squares!
            if (position.Piece.Color == PieceColor.White)
            {
                if (square.Equals(WhiteStartPosition))
                {
                    yield return new[] { new Move(position, new Square(6, 0), SpecialMove.CastleKing , null, new Square(5, 0), new[] { new Square(5, 0), new Square(6, 0) }) };
                    yield return new[] { new Move(position, new Square(2, 0), SpecialMove.CastleQueen , null, new Square(3, 0), new[] { new Square(2, 0), new Square(1, 0) }) };
                }
            }
            else
            {
                if (square.Equals(BlackStartPosition))
                {
                    yield return new[] { new Move(position, new Square(6, 7), SpecialMove.CastleKing , null, new Square(5, 7), new[] { new Square(5, 7), new Square(6, 7) }) };
                    yield return new[] { new Move(position, new Square(2, 7), SpecialMove.CastleQueen , null, new Square(3, 7), new[] { new Square(2, 7), new Square(1, 7) }) };
                }
            }
        }
    }

}
