using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine.Movement
{
    public abstract class MovementBase
    {
        public abstract IEnumerable<IEnumerable<Move>> GetTargetVectors(Square square);
    }
}
