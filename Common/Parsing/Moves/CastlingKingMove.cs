using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Parsing.Moves
{
    public class CastlingKingMove : CastlingMove
    {
        protected override Piece CastlingPiece => Piece.King;

        public override string ToString()
        {
            return $"[{base.ToString()}] castle kingside";
        }
    }
}
