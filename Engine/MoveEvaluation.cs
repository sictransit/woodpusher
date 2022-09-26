using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine
{
    internal class MoveEvaluation
    {
        public MoveEvaluation(Move move)
        {
            Move = move;
        }

        public ulong NodeCount { get; set; }

        public Move Move { get; set; }

        public int Score { get; set; }

        public override string ToString()
        {
            return $"{Move} {Score} / {NodeCount}";
        }
    }
}
