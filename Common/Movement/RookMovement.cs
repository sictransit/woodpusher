﻿using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Common.Movement
{
    public static class RookMovement
    {
        public static IEnumerable<IEnumerable<Move>> GetTargetVectors(Position position)
        {
            var square = position.Square;

            if (square.Rank < 7)
            {
                yield return Enumerable.Range(square.Rank + 1, 7 - square.Rank).Select(r => new Move(position, square.NewRank(r)));
            }

            if (square.File < 7)
            {
                yield return Enumerable.Range(square.File + 1, 7 - square.File).Select(f => new Move(position, square.NewFile(f)));
            }

            if (square.Rank > 0)
            {
                yield return Enumerable.Range(0, square.Rank).Reverse().Select(r => new Move(position, square.NewRank(r)));
            }

            if (square.File > 0)
            {
                yield return Enumerable.Range(0, square.File).Reverse().Select(f => new Move(position, square.NewFile(f)));
            }

        }
    }



}