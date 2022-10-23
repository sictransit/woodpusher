using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Parsing.Moves
{
    public class CastlingKingMove : CastlingMove
    {
        public CastlingKingMove(string raw) : base(raw)
        {
        }

        protected override Piece CastlingPiece => Piece.King;

        public override string ToString()
        {
            return $"[{base.ToString()}] castle kingside";
        }
    }
}
