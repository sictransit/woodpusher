using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Parsing.Moves
{
    public abstract class PGNMove
    {
        public abstract bool TryParse(string s, out PGNMove move);
    }
}
