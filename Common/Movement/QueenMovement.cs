using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Movement
{
    public static class QueenMovement
    {
        public static IEnumerable<IEnumerable<Move>> GetTargetVectors(Pieces position)
        {
            return BishopMovement.GetTargetVectors(position).Concat(RookMovement.GetTargetVectors(position));
        }
    }

}

