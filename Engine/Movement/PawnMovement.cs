
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Engine.Movement
{
    public static class PawnMovement
    {
        public static IEnumerable<IEnumerable<Move>> GetTargetVectors(Square square, PieceColour pieceColour)
        {
            var rank = square.Rank;
            var file = square.File;

            if (pieceColour == PieceColour.White)
            {
                if (rank < 7)
                {
                    yield return new[] { new Move(square.NewRank(rank + 1), rank == 6 ? MovementFlag.Promote : MovementFlag.None) };
                }

                if (rank < 7)
                {
                    if (Square.TryCreate(file - 1, rank + 1, out var takeLeft))
                    {
                        yield return new[] { new Move(takeLeft, rank == 6 ? MovementFlag.Promote : MovementFlag.None) };
                    }

                    if (Square.TryCreate(file + 1, rank + 1, out var takeRight))
                    {
                        yield return new[] { new Move(takeRight, rank == 6 ? MovementFlag.Promote : MovementFlag.None) };
                    }
                }

                if (rank == 1)
                {
                    yield return new[] { new Move(square.NewRank(rank + 1)), new Move(square.NewRank(rank + 2)) };
                }
            }
        }
    }


}