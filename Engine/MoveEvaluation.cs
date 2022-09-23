using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine
{
    internal class MoveEvaluation
    {
        public ulong NodeCount { get; set; }

        public Move Move { get; set; }

        public int Score { get; set; }
    }
}
