
using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine.Movement
{
    public static class QueenMovement
    {
        public static IEnumerable<IEnumerable<Target>> GetTargetVectors(Square square)
        {
            return BishopMovement.GetTargetVectors(square).Concat(RookMovement.GetTargetVectors(square));
        }
    }

}

