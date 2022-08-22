using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Model.Enums
{
    [Flags]
    public enum Castlings
    {
        WhiteKingside,
        WhiteQueenside, 
        BlackKingside,
        BlackQueenside,
    }
}
