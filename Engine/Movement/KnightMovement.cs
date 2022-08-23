using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine.Movement
{
    public class KnightMovement : MovementBase
    {
        public override IEnumerable<IEnumerable<Move>> GetTargetVectors(Square square)
        {
            var f = square.File;
            var r = square.Rank;

            Square s;

            if (Square.TryCreate(f + 1, r + 2, out s))
            {
                yield return new[] { new Move(s) };
            }

            if (Square.TryCreate(f + 2, r + 1, out s))
            {
                yield return new[] { new Move(s) };
            }

            if (Square.TryCreate(f + 2, r - 1, out s))
            {
                yield return new[] { new Move(s) };
            }

            if (Square.TryCreate(f + 1, r - 2, out s))
            {
                yield return new[] { new Move(s) };
            }

            if (Square.TryCreate(f - 1, r - 2, out s))
            {
                yield return new[] { new Move(s) };
            }

            if (Square.TryCreate(f - 2, r - 1, out s))
            {
                yield return new[] { new Move(s) };
            }

            if (Square.TryCreate(f - 2, r + 1, out s))
            {
                yield return new[] { new Move(s) };
            }

            if (Square.TryCreate(f - 1, r + 2, out s))
            {
                yield return new[] { new Move(s) };
            }
        }
    }
}
