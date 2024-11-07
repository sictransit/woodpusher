using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine
{
    public struct TranspositionTableEntry
    {
        public int Ply { get; }
        public Move? Move { get; }
        public int Score { get; }
        public ulong Hash { get; }
        public int MaxDepth { get; }

        public TranspositionTableEntry(int ply, Move? move, int score, ulong hash, int maxDepth)
        {
            Ply = ply;
            Move = move;
            Score = score;
            Hash = hash;
            MaxDepth = maxDepth;
        }
    }
}