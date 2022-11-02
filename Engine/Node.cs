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
            Sign = move.Piece.Is(Piece.White) ? 1 : -1;
            Score = -Sign * Scoring.MateScore * 2;
            MaxDepth = 2;
            Move = move;
        }

        public int MaxDepth { get; set; }

        public Move Move { get; }

        public int Score { get; set; }

        public int Count { get; set; }

        public int Sign { get; }

        public int AbsoluteScore => Score * Sign;

        public int? MateIn
        {
            get
            {
                var mateIn = Math.Abs(Math.Abs(Score) - Scoring.MateScore);

                if (mateIn <= Declarations.MaxDepth)
                {
                    mateIn += Sign == -1 ? 1 : 0;
                    var mateSign = AbsoluteScore > 0 ? 1 : -1;

                    return mateSign * mateIn / 2;
                }

                return null;
            }
        }

        public override string ToString()
        {
            return $"{Move} {Score}";
        }
    }
}
