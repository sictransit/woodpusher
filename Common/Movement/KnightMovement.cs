using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Movement
{
    public static class KnightMovement
    {
        public static IEnumerable<IEnumerable<Move>> GetTargetVectors(Piece piece)
        {
            var square = piece.GetSquare();

            foreach (var df in new[] { -2, -1, 1, 2 })
            {
                foreach (var dr in Math.Abs(df) == 2 ? new[] { -1, 1 } : new[] { -2, 2 })
                {
                    if (Square.TryCreate(square.File + df, square.Rank + dr, out var s))
                    {
                        yield return new[] { new Move(piece, s) };
                    }
                }
            }
        }
    }
}
