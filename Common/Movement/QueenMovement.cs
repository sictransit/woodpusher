using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Common.Movement
{
    public static class QueenMovement
    {
        public static IEnumerable<IEnumerable<Move>> GetTargetVectors(Position position)
        {
            return BishopMovement.GetTargetVectors(position).Concat(RookMovement.GetTargetVectors(position));
        }
    }

}

