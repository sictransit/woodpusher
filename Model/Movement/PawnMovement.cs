using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model.Movement
{
    public static class PawnMovement
    {
        public static IEnumerable<IEnumerable<Target>> GetTargetVectors(Square square, PieceColor color)
        {
            var rank = square.Rank;

            if (rank is 7 or 0)
            {
                yield break;
            }

            var file = square.File;

            if (color == PieceColor.White)
            {
                var forward = new List<Target> { new(square.NewRank(rank + 1), rank == 6 ? SpecialMove.Promote | SpecialMove.CannotTake : SpecialMove.CannotTake) };

                if (rank == 1)
                {
                    forward.Add(new Target(square.NewRank(rank + 2), SpecialMove.CannotTake, square.NewRank(rank + 1)));
                }

                yield return forward;

                if (Square.TryCreate(file - 1, rank + 1, out var takeLeft))
                {
                    if (rank == 6)
                    {
                        foreach (var promotionType in new[] { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight })
                        {
                            yield return new[] { new Target(takeLeft, SpecialMove.MustTake | SpecialMove.Promote, promotionType: promotionType) };
                        }
                    }
                    else
                    {
                        yield return new[] { new Target(takeLeft, SpecialMove.MustTake) };
                    }

                    if (rank == 4)
                    {
                        yield return new[] { new Target(takeLeft, SpecialMove.EnPassant) };
                    }
                }

                if (Square.TryCreate(file + 1, rank + 1, out var takeRight))
                {
                    if (rank == 6)
                    {
                        foreach (var promotionType in new[] { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight })
                        {
                            yield return new[] { new Target(takeRight, SpecialMove.MustTake | SpecialMove.Promote, promotionType: promotionType) };
                        }
                    }
                    else
                    {
                        yield return new[] { new Target(takeRight, SpecialMove.MustTake) };
                    }

                    if (rank == 4)
                    {
                        yield return new[] { new Target(takeRight, SpecialMove.EnPassant) };
                    }
                }
            }
            else
            {
                var forward = new List<Target> { new(square.NewRank(rank - 1), rank == 1 ? SpecialMove.Promote | SpecialMove.CannotTake : SpecialMove.CannotTake) };

                if (rank == 6)
                {
                    forward.Add(new Target(square.NewRank(rank - 2), SpecialMove.CannotTake, square.NewRank(rank - 1)));
                }

                yield return forward;

                if (Square.TryCreate(file - 1, rank - 1, out var takeLeft))
                {
                    if (rank == 1)
                    {
                        foreach (var promotionType in new[] { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight })
                        {
                            yield return new[] { new Target(takeLeft, SpecialMove.MustTake | SpecialMove.Promote, promotionType: promotionType) };
                        }
                    }
                    else
                    {
                        yield return new[] { new Target(takeLeft, SpecialMove.MustTake ) };
                    }

                    if (rank == 3)
                    {
                        yield return new[] { new Target(takeLeft, SpecialMove.EnPassant) };
                    }
                }

                if (Square.TryCreate(file + 1, rank - 1, out var takeRight))
                {
                    if (rank == 1)
                    {
                        foreach (var promotionType in new[] { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight })
                        {
                            yield return new[] { new Target(takeRight, SpecialMove.MustTake | SpecialMove.Promote, promotionType: promotionType) };
                        }
                    }
                    else
                    {
                        yield return new[] { new Target(takeRight, SpecialMove.MustTake) };
                    }

                    if (rank == 3)
                    {
                        yield return new[] { new Target(takeRight, SpecialMove.EnPassant) };
                    }
                }
            }
        }
    }
}