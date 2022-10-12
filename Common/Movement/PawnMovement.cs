using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Movement
{
    public static class PawnMovement
    {
        public static IEnumerable<IEnumerable<Move>> GetTargetVectors(Pieces piece)
        {
            var square = piece.GetSquare();
            var rank = square.Rank;

            if (rank is 7 or 0)
            {
                yield break;
            }

            int dRank;
            int promoteRank;
            int doubleStepRank;
            int enPassantRank;

            if (piece.Is(Pieces.White))
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
                        foreach (var promotionType in new[] { Pieces.Queen, Pieces.Rook, Pieces.Bishop, Pieces.Knight })
                        {
                            yield return new[] { new Move(piece, promoteSquare.ToMask(), SpecialMove.Promote | (dFile == 0 ? SpecialMove.CannotTake : SpecialMove.MustTake), promotionType: promotionType) };
                        }
                    }
                }
            }
            else
            {
                yield return new[] {
                new Move(piece, square.NewRank(rank + dRank), SpecialMove.CannotTake) }.Concat(
                    rank == doubleStepRank
                    ? new[] { new Move(piece, square.NewRank(rank + dRank * 2).ToMask(), SpecialMove.CannotTake, square.NewRank(rank + dRank).ToMask()) }
                    : Enumerable.Empty<Move>()
                    );

                foreach (var dFile in new[] { -1, 1 })
                {
                    if (Square.TryCreate(file + dFile, rank + dRank, out var takeSquare))
                    {
                        yield return new[] { new Move(piece, takeSquare, SpecialMove.MustTake) };
                    }
                }

                if (rank == enPassantRank)
                {
                    foreach (var dFile in new[] { -1, 1 })
                    {
                        if (Square.TryCreate(file + dFile, rank + dRank, out var enPassantSquare))
                        {
                            yield return new[] { new Move(piece, enPassantSquare.ToMask(), SpecialMove.EnPassant, enPassantSquare.ToMask()) };
                        }
                    }
                }
            }
        }
    }
}