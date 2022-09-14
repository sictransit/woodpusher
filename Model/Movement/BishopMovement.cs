namespace SicTransit.Woodpusher.Model.Movement
{
    public static class BishopMovement
    {
        public static IEnumerable<IEnumerable<Target>> GetTargetVectors(Square square)
        {
            var topRight = Math.Max(square.File, square.Rank);

            if (topRight < 7)
            {
                yield return Enumerable.Range(1, 7 - topRight).Select(d => new Target(square.AddFileAndRank(d, d)));
            }

            var bottomRight = Math.Max(square.File, 7 - square.Rank);

            if (bottomRight < 7)
            {
                yield return Enumerable.Range(1, 7 - bottomRight).Select(d => new Target(square.AddFileAndRank(d, -d)));
            }

            var bottomLeft = Math.Min(square.File, square.Rank);

            if (bottomLeft > 0)
            {
                yield return Enumerable.Range(1, bottomLeft).Select(d => new Target(square.AddFileAndRank(-d, -d)));
            }

            var topLeft = Math.Min(square.File, 7 - square.Rank);

            if (topLeft > 0)
            {
                yield return Enumerable.Range(1, topLeft).Select(d => new Target(square.AddFileAndRank(-d, d)));
            }
        }

    }


}