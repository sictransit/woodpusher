
using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine.Movement
{
    public static class BishopMovement
    {
        public static IEnumerable<IEnumerable<Move>> GetTargetVectors(Square square)
        {
            var topRight = Math.Max(square.File, square.Rank);

            if (topRight < 7)
            {
                yield return Enumerable.Range(1, 7 - topRight).Select(d => new Move(square.AddFileAndRank(d, d)));
            }

            var bottomRight = Math.Max(square.File, 7 - square.Rank);

            if (bottomRight < 7)
            {
                yield return Enumerable.Range(1, 7 - bottomRight).Select(d => new Move(square.AddFileAndRank(d, -d)));
            }

            var bottomLeft = Math.Min(square.File, square.Rank);

            if (bottomLeft > 0)
            {
                yield return Enumerable.Range(1, bottomLeft).Select(d => new Move(square.AddFileAndRank(-d, -d)));
            }

            var topLeft = Math.Min(7 - square.File, square.Rank);

            if (topLeft > 0)
            {
                yield return Enumerable.Range(1, topLeft).Select(d => new Move(square.AddFileAndRank(-d, d)));
            }
        }

    }


}