using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Movement
{
    public static class BishopMovement
    {
        public static IEnumerable<IEnumerable<Move>> GetTargetVectors(Piece piece)
        {
            var square = piece.GetSquare();

            var topRight = Math.Max(square.File, square.Rank);

            if (topRight < 7)
            {
                yield return Enumerable.Range(1, 7 - topRight).Select(d => new Move(piece, square.AddFileAndRank(d, d)));
            }

            var bottomRight = Math.Max(square.File, 7 - square.Rank);

            if (bottomRight < 7)
            {
                yield return Enumerable.Range(1, 7 - bottomRight).Select(d => new Move(piece, square.AddFileAndRank(d, -d)));
            }

            var bottomLeft = Math.Min(square.File, square.Rank);

            if (bottomLeft > 0)
            {
                yield return Enumerable.Range(1, bottomLeft).Select(d => new Move(piece, square.AddFileAndRank(-d, -d)));
            }

            var topLeft = Math.Min(square.File, 7 - square.Rank);

            if (topLeft > 0)
            {
                yield return Enumerable.Range(1, topLeft).Select(d => new Move(piece, square.AddFileAndRank(-d, d)));
            }
        }

    }


}