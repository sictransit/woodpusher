using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Engine
{
    public class Node
    {
        public Node(Move move)
        {
            Move = move;
            Sign = move.Position.Piece.Color == PieceColor.White ? 1 : -1;
            Score = -Sign * Patzer.MATE_SCORE * 2;
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
