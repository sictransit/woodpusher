using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Model
{
    public struct BitField
    {
        public ulong Aggregate => Pawn | Rook | Knight | Bishop | Queen | King;

        public ulong Pawn;
        public ulong Rook;
        public ulong Knight;
        public ulong Bishop;
        public ulong Queen;
        public ulong King;
    }
}
