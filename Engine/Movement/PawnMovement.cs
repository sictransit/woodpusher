
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Engine.Movement
{
    public static class PawnMovement
    {
        public static IEnumerable<IEnumerable<Move>> GetTargetVectors(Square square, PieceColour colour)
        {
            var rank = square.Rank;

            if (rank is 7 or 0)
            {
                yield break;
            }

            var file = square.File;

            if (colour == PieceColour.White)
            {
                var forward = new List<Move>() { new Move(square.NewRank(rank + 1), rank == 6 ? SpecialMove.Promote | SpecialMove.CannotTake : SpecialMove.CannotTake) };

                if (rank == 1)
                {
                    forward.Add(new Move(square.NewRank(rank + 2), SpecialMove.CannotTake));
                }

                yield return forward;

                if (Square.TryCreate(file - 1, rank + 1, out var takeLeft))
                {
                    yield return new[] { new Move(takeLeft, rank == 6 ? SpecialMove.MustTake | SpecialMove.Promote : SpecialMove.MustTake) };

                    if (rank == 4)
                    {
                        yield return new[] { new Move(takeLeft, SpecialMove.EnPassant) };
                    }
                }

                if (Square.TryCreate(file + 1, rank + 1, out var takeRight))
                {
                    yield return new[] { new Move(takeRight, rank == 6 ? SpecialMove.MustTake | SpecialMove.Promote : SpecialMove.MustTake) };

                    if (rank == 4)
                    {
                        yield return new[] { new Move(takeRight, SpecialMove.EnPassant) };
                    }
                }
            }
            else
            {
                var forward = new List<Move>() { new Move(square.NewRank(rank - 1), rank == 1 ? SpecialMove.Promote | SpecialMove.CannotTake : SpecialMove.CannotTake) };

                if (rank == 6)
                {
                    forward.Add(new Move(square.NewRank(rank - 2), SpecialMove.CannotTake));
                }

                yield return forward;

                if (Square.TryCreate(file - 1, rank - 1, out var takeLeft))
                {
                    yield return new[] { new Move(takeLeft, rank == 1 ? SpecialMove.MustTake | SpecialMove.Promote : SpecialMove.MustTake) };

                    if (rank == 3)
                    {
                        yield return new[] { new Move(takeLeft, SpecialMove.EnPassant) };
                    }
                }

                if (Square.TryCreate(file + 1, rank - 1, out var takeRight))
                {
                    yield return new[] { new Move(takeRight, rank == 1 ? SpecialMove.MustTake | SpecialMove.Promote : SpecialMove.MustTake) };

                    if (rank == 3)
                    {
                        yield return new[] { new Move(takeRight, SpecialMove.EnPassant) };
                    }
                }
            }
        }
    }
}