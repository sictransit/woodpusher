using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Parsing.Moves
{
    public class CastlingQueenMove : CastlingMove
    {
        protected override Piece CastlingPiece => Piece.Queen;

        public override string ToString()
        {
            return $"[{base.ToString()}] castle queenside";
        }
    }
}
