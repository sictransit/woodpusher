using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Engine.Enums;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Engine
{
    public class Node
    {
        public Node(IBoard board, Move move)
        {
            Sign = move.Piece.Is(Piece.White) ? 1 : -1;
            Score = Sign * Declarations.MateScore / 2;
            Move = move;
            Status = NodeStatus.Waiting;
            Board = board.Play(move);
            MaxDepth = 2;
        }

        public IBoard Board { get; }

        public int MaxDepth { get; set; }

        public NodeStatus Status { get; set; }

        public Move Move { get; }

        public Move? PonderMove { get; set; }

        public int Score { get; set; }

        public int Count { get; set; }

        private int Sign { get; }

        public int AbsoluteScore => Score * Sign;

        public int? MateIn
        {
            get
            {
                var mateIn = Math.Abs(Math.Abs(Score) - Declarations.MateScore);

                mateIn += Sign == -1 ? 1 : 0;

                if (mateIn <= Declarations.MaxDepth)
                {
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
