namespace SicTransit.Woodpusher.Common.Lookup
{
    public class ThreatMasks
    {
        public ulong Pawn { get; }
        public ulong Rook { get; }
        public ulong Knight { get; }
        public ulong Bishop { get; }
        public ulong Queen { get; }
        public ulong King { get; }
        public ulong All { get; }

        public ThreatMasks(ulong pawn, ulong rook, ulong knight, ulong bishop, ulong queen, ulong king)
        {
            Pawn = pawn;
            Rook = rook;
            Knight = knight;
            Bishop = bishop;
            Queen = queen;
            King = king;
            All = pawn | rook | knight | bishop | queen | king;
        }
    }
}
