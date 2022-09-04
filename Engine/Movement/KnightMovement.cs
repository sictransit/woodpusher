using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine.Movement
{
    public static class KnightMovement
    {
        public static IEnumerable<IEnumerable<Target>> GetTargetVectors(Square square)
        {
            var f = square.File;
            var r = square.Rank;

            Square s;

            if (Square.TryCreate(f + 1, r + 2, out s))
            {
                yield return new[] { new Target(s) };
            }

            if (Square.TryCreate(f + 2, r + 1, out s))
            {
                yield return new[] { new Target(s) };
            }

            if (Square.TryCreate(f + 2, r - 1, out s))
            {
                yield return new[] { new Target(s) };
            }

            if (Square.TryCreate(f + 1, r - 2, out s))
            {
                yield return new[] { new Target(s) };
            }

            if (Square.TryCreate(f - 1, r - 2, out s))
            {
                yield return new[] { new Target(s) };
            }

            if (Square.TryCreate(f - 2, r - 1, out s))
            {
                yield return new[] { new Target(s) };
            }

            if (Square.TryCreate(f - 2, r + 1, out s))
            {
                yield return new[] { new Target(s) };
            }

            if (Square.TryCreate(f - 1, r + 2, out s))
            {
                yield return new[] { new Target(s) };
            }
        }
    }
}
