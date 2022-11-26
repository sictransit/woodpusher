using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Model
{
    public class BestMove
    {
        public BestMove(AlgebraicMove move, AlgebraicMove? ponder = null)
        {
            Move = move;
            Ponder = ponder;
        }

        public AlgebraicMove Move { get; }
        public AlgebraicMove? Ponder { get; }
    }
}
