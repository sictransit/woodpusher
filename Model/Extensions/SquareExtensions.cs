using System.Numerics;

namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class SquareExtensions
    {
        public static ulong ToMask(this Square square) => 1ul << (square.Rank << 3) + square.File;

        public static ulong ToMask(this IEnumerable<Square> squares)
        {
            return squares.Aggregate(0ul, (a, b) => a | b.ToMask());
        }

        public static ulong AddFileAndRank(this ulong mask, int fileDelta, int rankDelta)
        {
            var shift = rankDelta * 8 + fileDelta;

            return shift < 0 ? mask >> -shift : mask << shift;
        }

        public static Square ToSquare(this ulong mask)
        {
            if (mask == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(mask));
            }

            var trailingZeroes = BitOperations.TrailingZeroCount(mask);

            return new Square(trailingZeroes % 8, trailingZeroes / 8);
        }

        public static IEnumerable<Square> ToSquares(this ulong mask)
        {
            for (var i = 0; i < 64; i++)
            {
                var test = 1ul << i;
                if ((test & mask) != 0)
                {
                    yield return ToSquare(test);
                }
            }
        }

        public static IEnumerable<Square> ToTravelPath(this Square square, Square target)
        {
            var deltaFile = target.File - square.File;
            var deltaRank = target.Rank - square.Rank;

            var fileSteps = Math.Abs(deltaFile);
            var rankSteps = Math.Abs(deltaRank);

            if (fileSteps != rankSteps && fileSteps != 0 && rankSteps != 0)
            {
                // Hopefully a knight move, i.e. return Empty.
                yield break;
            }

            var distance = Math.Max(fileSteps, rankSteps);

            if (distance < 2)
            {
                // No squares in between.
                yield break;
            }

            var df = Math.Sign(deltaFile);
            var dr = Math.Sign(deltaRank);

            for (var step = 1; step < distance; step++)
            {
                yield return square.AddFileAndRank(df * step, dr * step);
            }
        }

        public static string ToAlgebraicNotation(this Square square)
        {
            var f = (char)(square.File + 'a');
            var r = (char)(square.Rank + '1');

            return $"{f}{r}";
        }

        public static IEnumerable<Square> AllSquares => Enumerable.Range(0, 8).Select(file => Enumerable.Range(0, 8).Select(rank => new Square(file, rank))).SelectMany(s => s);
    }
}
