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

        public Square(string algebraic) : this(algebraic[0].ToFile(), algebraic[1].ToRank())
        { }

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
