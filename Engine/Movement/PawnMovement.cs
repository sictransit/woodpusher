
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Engine.Movement
{
    public static class PawnMovement
    {
        public static IEnumerable<IEnumerable<Move>> GetTargetVectors(Square square, PieceColour pieceColour)
        {
            var rank = square.Rank;

            if (rank is 7 or 0)
            {
                throw new ArgumentOutOfRangeException(nameof(square), $"A pawn shouldn't be here: {square}");
            }

            var file = square.File;

            if (pieceColour == PieceColour.White)
            {
                var forward = new List<Move>() { new Move(square.NewRank(rank + 1), rank == 6 ? MovementFlags.Promote : MovementFlags.None) };

                if (rank == 1)
                {
                    forward.Add(new Move(square.NewRank(rank + 2)));
                }

                yield return forward;

                var mustTake = MovementFlags.MustTake;

                if (Square.TryCreate(file - 1, rank + 1, out var takeLeft))
                {
                    yield return new[] { new Move(takeLeft, rank == 6 ? mustTake | MovementFlags.Promote : mustTake) };

                    if (rank > 0 && rank < 5)
                    {
                        yield return new[] { new Move(takeLeft, MovementFlags.EnPassant) };
                    }
                }

                if (Square.TryCreate(file + 1, rank + 1, out var takeRight))
                {
                    yield return new[] { new Move(takeRight, rank == 6 ? mustTake | MovementFlags.Promote : mustTake) };

                    if (rank > 0 && rank < 5)
                    {
                        yield return new[] { new Move(takeRight, MovementFlags.EnPassant) };
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}