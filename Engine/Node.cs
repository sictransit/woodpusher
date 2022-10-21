using SicTransit.Woodpusher.Common.Lookup;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Engine
{
    public class Node
    {
        public Node(Move move, int maxDepth)
        {
            Sign = move.Piece.Is(Piece.White) ? 1 : -1;
            Score = -Sign * Scoring.MateScore * 2;
            Line = new Move[maxDepth];
            Line[0] = move;
        }

        public Move[] Line { get; }

        public IEnumerable<Move> GetLine() => Line.TakeWhile(m => m != null);

        public Move Move => Line[0];

        public int Score { get; set; }

        public int Sign { get; }

        public int AbsoluteScore => Score * Sign;

        public override string ToString()
        {
            return $"{Move} {Score}";
        }
    }
}
