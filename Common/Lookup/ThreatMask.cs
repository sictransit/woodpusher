namespace SicTransit.Woodpusher.Common.Lookup
{
    public class ThreatMask
    {
        public ulong PawnMask { get; }
        public ulong RookMask { get; }
        public ulong KnightMask { get; }
        public ulong BishopMask { get; }
        public ulong QueenMask { get; }
        public ulong KingMask { get; }

        public ThreatMask(ulong pawnMask, ulong rookMask, ulong knightMask, ulong bishopMask, ulong queenMask, ulong kingMask)
        {
            PawnMask = pawnMask;
            RookMask = rookMask;
            KnightMask = knightMask;
            BishopMask = bishopMask;
            QueenMask = queenMask;
            KingMask = kingMask;
        }
    }
}
