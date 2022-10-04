using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine
{
    internal class Node
    {
        public Node(Move move)
        {
            Move = move;
            Score = int.MinValue;
        }

        public Move Move { get; }

        public int Score { get; set; }

        public override string ToString()
        {
            return $"{Move} {Score}";
        }

    }
}
