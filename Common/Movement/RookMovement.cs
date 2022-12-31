using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Movement
{
    public static class RookMovement
    {
        public static IEnumerable<IEnumerable<Move>> GetTargetVectors(Piece piece)
        {
            var square = piece.GetSquare();

            if (square.Rank < 7)
            {
                yield return Enumerable.Range(square.Rank + 1, 7 - square.Rank).Select(r => new Move(piece, square.NewRank(r)));
            }

            if (square.File < 7)
            {
                yield return Enumerable.Range(square.File + 1, 7 - square.File).Select(f => new Move(piece, square.NewFile(f)));
            }

            if (square.Rank > 0)
            {
                yield return Enumerable.Range(0, square.Rank).Reverse().Select(r => new Move(piece, square.NewRank(r)));
            }

            if (square.File > 0)
            {
                yield return Enumerable.Range(0, square.File).Reverse().Select(f => new Move(piece, square.NewFile(f)));
            }
        }
    }
}
