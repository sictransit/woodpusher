namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class SquareExtensions
    {
        public static ulong ToMask(this Square square) => 1ul << ((square.Rank << 3) + square.File);

        public static Square ToSquare(this ulong mask)
        {
            if (mask == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(mask));
            }

            int rank = 0;
            int file = 0;

            for (int shift = 0; shift < 64; shift++)
            {
                if (mask == 1)
                {
                    return new Square(file, rank);
                }

                mask >>= 1;

                file++;

                if (file == 8)
                {
                    rank++;
                    file = 0;
                }
            }

            throw new NotImplementedException("Wow! Bit manipulation is broken in C#. We should never get here!");
        }

    }
}
