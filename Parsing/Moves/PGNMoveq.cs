using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Parsing.Moves
{
    public abstract class PGNMoveq
    {
        public abstract bool TryParse(string s, out PGNMoveq move);
    }
}
