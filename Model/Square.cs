using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model
{
    public struct Square
    {
        public int File;
        public int Rank;

        public Square(int file, int rank)
        {
            if (file is < 0 or > 7) throw new ArgumentOutOfRangeException(nameof(file));
            if (rank is < 0 or > 7) throw new ArgumentOutOfRangeException(nameof(rank));

            File = file;
            Rank = rank;
        }

        public static Square FromAlgebraicNotation(string algebraicNotation)
        {
            if (!StringExtensions.IsAlgebraicNotation(algebraicNotation)) throw new ArgumentOutOfRangeException(nameof(algebraicNotation));

            var file = algebraicNotation[0] - 'a';
            var rank = algebraicNotation[1] - '1';

            return new Square(file, rank);
        }

        public string ToAlgebraicNotation()
        {
            var f = (char)(File + 'a');
            var r = (char)(Rank + '1');

            return $"{f}{r}";
        }

        public override string ToString()
        {
            return ToAlgebraicNotation();
        }
    }
}
