using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace woodpusher.model
{
    public class Ply
    {
        public Ply(Board board)
        {
            Board = board;
        }

        public Board Board { get; }
    }
}
