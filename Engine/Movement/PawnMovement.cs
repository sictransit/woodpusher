
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Engine.Movement
{
    public static class PawnMovement
    {
        public static IEnumerable<IEnumerable<Move>> GetTargetVectors(Square square, Piece colour)
        {
            var rank = square.Rank;

            if (rank is 7 or 0)
            {
                throw new ArgumentOutOfRangeException(nameof(square), $"A pawn shouldn't be here: {square}");
            }

            var file = square.File;

            if (colour == Piece.White)
            {
                var forward = new List<Move>() { new Move(square.NewRank(rank + 1), rank == 6 ? MovementFlags.Promote : MovementFlags.None) };

                if (rank == 1)
                {
                    forward.Add(new Move(square.NewRank(rank + 2)));
                }

                yield return forward;

                if (Square.TryCreate(file - 1, rank + 1, out var takeLeft))
                {
                    yield return new[] { new Move(takeLeft, rank == 6 ? MovementFlags.MustTake | MovementFlags.Promote : MovementFlags.MustTake) };

                    if (rank == 4)
                    {
                        yield return new[] { new Move(takeLeft, MovementFlags.EnPassant) };
                    }
                }

                if (Square.TryCreate(file + 1, rank + 1, out var takeRight))
                {
                    yield return new[] { new Move(takeRight, rank == 6 ? MovementFlags.MustTake | MovementFlags.Promote : MovementFlags.MustTake) };

                    if (rank == 4)
                    {
                        yield return new[] { new Move(takeRight, MovementFlags.EnPassant) };
                    }
                }
            }
            else
            {
                var forward = new List<Move>() { new Move(square.NewRank(rank - 1), rank == 1 ? MovementFlags.Promote : MovementFlags.None) };

                if (rank == 6)
                {
                    forward.Add(new Move(square.NewRank(rank - 2)));
                }

                yield return forward;

                if (Square.TryCreate(file - 1, rank - 1, out var takeLeft))
                {
                    yield return new[] { new Move(takeLeft, rank == 1 ? MovementFlags.MustTake | MovementFlags.Promote : MovementFlags.MustTake) };

                    if (rank == 3)
                    {
                        yield return new[] { new Move(takeLeft, MovementFlags.EnPassant) };
                    }
                }

                if (Square.TryCreate(file + 1, rank - 1, out var takeRight))
                {
                    yield return new[] { new Move(takeRight, rank == 1 ? MovementFlags.MustTake | MovementFlags.Promote : MovementFlags.MustTake) };

                    if (rank == 3)
                    {
                        yield return new[] { new Move(takeRight, MovementFlags.EnPassant) };
                    }
                }
            }
        }
    }
}