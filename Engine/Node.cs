using SicTransit.Woodpusher.Common.Lookup;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Engine
{
    public class Node
    {
        public Node(Move move)
        {
            Move = move;
            Sign = move.Piece.Is(Piece.White) ? 1 : -1;
            Score = -Sign * Scoring.MateScore * 2;
        }

        public Move Move { get; }

        public int Score { get; set; }

        public int Sign { get; }

        public int AbsoluteScore => Score * Sign;

        public override string ToString()
        {
            return $"{Move} {Score}";
        }

    }
}
