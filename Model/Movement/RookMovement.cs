namespace SicTransit.Woodpusher.Model.Movement
{
    public static class RookMovement
    {
        public static IEnumerable<IEnumerable<Target>> GetTargetVectors(Square square)
        {
            if (square.Rank < 7)
            {
                yield return Enumerable.Range(square.Rank + 1, 7 - square.Rank).Select(r => new Target(square.NewRank(r)));
            }

            if (square.File < 7)
            {
                yield return Enumerable.Range(square.File + 1, 7 - square.File).Select(f => new Target(square.NewFile(f)));
            }

            if (square.Rank > 0)
            {
                yield return Enumerable.Range(0, square.Rank).Reverse().Select(r => new Target(square.NewRank(r)));
            }

            if (square.File > 0)
            {
                yield return Enumerable.Range(0, square.File).Reverse().Select(f => new Target(square.NewFile(f)));
            }

        }
    }

}
