using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model
{
    public struct Square
    {
        public int File { get; set; }
        public int Rank { get; set; }

        public Square(int file, int rank)
        {
            if (file is < 0 or > 7) throw new ArgumentOutOfRangeException(nameof(file));
            if (rank is < 0 or > 7) throw new ArgumentOutOfRangeException(nameof(rank));

            File = file;
            Rank = rank;
        }

        public static bool TryCreate(int file, int rank, out Square square)
        {
            square = default;

            if ((file is < 0 or > 7) || (rank is < 0 or > 7))
            {
                return false;
            }

            square = new Square(file, rank);

            return true;
        }

        public static Square FromAlgebraicNotation(string algebraicNotation)
        {
            if (!StringExtensions.IsAlgebraicNotation(algebraicNotation)) throw new ArgumentOutOfRangeException(nameof(algebraicNotation));

            var file = algebraicNotation[0].ToFile();
            var rank = algebraicNotation[1].ToRank();

            return new Square(file, rank);
        }

        public string ToAlgebraicNotation()
        {
            var f = (char)(File + 'a');
            var r = (char)(Rank + '1');

            return $"{f}{r}";
        }

        public Square NewRank(int rank) => new(File, rank);

        public Square NewFile(int file) => new(file, Rank);

        public Square AddFileAndRank(int fileDelta, int rankDelta) => new(File + fileDelta, Rank + rankDelta);

        public override string ToString()
        {
            return ToAlgebraicNotation();
        }
    }
}
