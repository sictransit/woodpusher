using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model.Movement
{
    public static class PawnMovement
    {
        public static IEnumerable<IEnumerable<Move>> GetTargetVectors(Position position)
        {
            var square = position.Square;
            var rank = square.Rank;

            if (rank is 7 or 0)
            {
                yield break;
            }

            int dRank;
            int promoteRank;
            int doubleStepRank;
            int enPassantRank;

            if (position.Piece.Color == PieceColor.White)
            {
                dRank = 1;
                promoteRank = 6;
                doubleStepRank = 1;
                enPassantRank = 4;
            }
            else
            {
                dRank = -1;
                promoteRank = 1;
                doubleStepRank = 6;
                enPassantRank = 3;
            }

            var file = square.File;

            if (rank == promoteRank)
            {
                foreach (var dFile in new[] { -1, 0, 1 })
                {
                    if (Square.TryCreate(file + dFile, rank + dRank, out var promoteSquare))
                    {
                        foreach (var promotionType in new[] { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight })
                        {
                            yield return new[] { new Target(promoteSquare, SpecialMove.Promote | (dFile == 0 ? SpecialMove.CannotTake : SpecialMove.MustTake), promotionType: promotionType).ToMove(position) };
                        }
                    }
                }
            }
            else
            {
                yield return new[] {
                new Target(square.NewRank(rank + dRank), SpecialMove.CannotTake) }.Concat(
                    rank == doubleStepRank
                    ? new[] { new Target(square.NewRank(rank + dRank * 2), SpecialMove.CannotTake, square.NewRank(rank + dRank)) }
                    : Enumerable.Empty<Target>()
                    ).Select(t => t.ToMove(position));

                foreach (var dFile in new[] { -1, 1 })
                {
                    if (Square.TryCreate(file + dFile, rank + dRank, out var takeSquare))
                    {
                        yield return new[] { new Target(takeSquare, SpecialMove.MustTake).ToMove(position) };
                    }
                }

                if (rank == enPassantRank)
                {
                    foreach (var dFile in new[] { -1, 1 })
                    {
                        if (Square.TryCreate(file + dFile, rank + dRank, out var enPassantSquare))
                        {
                            yield return new[] { new Target(enPassantSquare, SpecialMove.EnPassant, enPassantSquare).ToMove(position) };
                        }
                    }
                }
            }
        }
    }
}