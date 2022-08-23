using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine.Movement
{
    public abstract class MovementBase
    {
        public abstract int Directions { get; }

        public abstract IEnumerable<Square> GetTargetSquares(Square square, int direction);
    }
}
