using SicTransit.Woodpusher.Common.Lookup;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Engine
{
    internal class Node
    {
        public Node(Move move)
        {
            Move = move;            
            Sign = move.Position.Piece.Color == PieceColor.White ? 1 : -1;
            Score = -Sign * Patzer.MATE_SCORE;
        }

        public Move Move { get; }

        public int Score { get; set; }

        public int Sign { get; }        

        public void UpdateScoreIfBetter(int score)
        {
            if ((score * Sign) > AbsoluteScore)
            {
                Score = score;
            }
        }

        public int AbsoluteScore => Score * Sign;

        public override string ToString()
        {
            return $"{Move} {Score}";
        }

    }
}
