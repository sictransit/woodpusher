using System.Numerics;

namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class SquareExtensions
    {
        public static ulong ToMask(this Square square) => 1ul << (square.Rank << 3) + square.File;

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
            for (int i = 0; i < 64; i++)
            {
                var test = 1ul << i;
                if ((test & mask) != 0)
                {
                    yield return ToSquare(test);
                }
            }
        }
    }
}
