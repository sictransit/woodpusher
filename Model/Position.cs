using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model
{
    public struct Position
    {
        public int File;
        public int Rank;

        public Position(int file, int rank)
        {
            if (file is < 0 or > 7) throw new ArgumentOutOfRangeException(nameof(file));
            if (rank is < 0 or > 7) throw new ArgumentOutOfRangeException(nameof(rank));

            File = file;
            Rank = rank;
        }

        public static Position FromAlgebraicNotation(string algebraicNotation)
        {
            if (!StringExtensions.IsAlgebraicNotation(algebraicNotation)) throw new ArgumentOutOfRangeException(nameof(algebraicNotation));

            var file = algebraicNotation[0] - 'a';
            var rank = algebraicNotation[1] - '1';

            return new Position(file, rank);
        }
    }
}
