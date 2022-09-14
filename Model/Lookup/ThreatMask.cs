namespace SicTransit.Woodpusher.Model.Lookup
{
    public struct ThreatMask
    {
        public ulong PawnMask { get; init; }
        public ulong RookMask { get; init; }
        public ulong KnightMask { get; init; }
        public ulong BishopMask { get; init; }
        public ulong QueenMask { get; init; }
        public ulong KingMask { get; init; }

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
