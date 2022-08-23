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
            square = new Square(file, rank);

            if ((file is < 0 or > 7) || (rank is < 0 or > 7))
            {
                return false;
            }

            return true;
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

        public Square NewRank(int rank) => new(File, rank);

        public Square NewFile(int file) => new(file, Rank);        

        public Square AddFileAndRank(int fileDelta, int rankDelta) => new Square(File+fileDelta, Rank+rankDelta);

        //public int DistanceToEgde()
        //{
        //    return Math.Min(7 - Math.Max(File, Rank), Math.Min(File, Rank));
        //}

        public override string ToString()
        {
            return ToAlgebraicNotation();
        }
    }
}
