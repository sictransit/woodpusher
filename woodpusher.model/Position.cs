using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace woodpusher.model
{
    internal struct Position
    {
        public int File;
        public int Rank;

        public Position(int file, int rank)
        {
            if (file is < 0 or > 7) throw new ArgumentException(nameof(file));
            if (rank is < 0 or > 7) throw new ArgumentException(nameof(rank));

            File = file;
            Rank = rank;
        }
    }
}
