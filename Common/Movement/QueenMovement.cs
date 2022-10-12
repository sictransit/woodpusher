using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Movement
{
    public static class QueenMovement
    {
        public static IEnumerable<IEnumerable<Move>> GetTargetVectors(Pieces piece)
        {
            return BishopMovement.GetTargetVectors(piece).Concat(RookMovement.GetTargetVectors(piece));
        }
    }

}

