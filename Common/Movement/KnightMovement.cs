﻿using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Common.Movement
{
    public static class KnightMovement
    {
        public static IEnumerable<IEnumerable<Move>> GetTargetVectors(Position position)
        {
            var square = position.Square;

            foreach (var df in new[] { -2, -1, 1, 2 })
            {
                foreach (var dr in Math.Abs(df) == 2 ? new[] { -1, 1 } : new[] { -2, 2 })
                {
                    if (Square.TryCreate(square.File + df, square.Rank + dr, out var s))
                    {
                        yield return new[] { new Move(position, s) };
                    }
                }
            }
        }
    }
}