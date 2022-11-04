using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine
{
    internal class PrincipalVariationNode
    {
        public PrincipalVariationNode(Move move, int score)
        {
            Move = move;
            Score = score;
        }

        public Move Move { get; set; }
        public int Score { get; set; }
    }
}
