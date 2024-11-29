using SicTransit.Woodpusher.Engine.Enum;
using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine
{
    internal readonly struct TranspositionTableEntry
    {
        public Move Move { get; }
        public int Score { get; }
        public ulong Hash { get; }
        public int Depth { get; }
        public EntryType EntryType { get; }

        public TranspositionTableEntry(EntryType entryType, Move move, int score, ulong hash, int depth)
        {
            Move = move;
            Score = score;
            Hash = hash;
            Depth = depth;
            EntryType = entryType;
        }
    }
}