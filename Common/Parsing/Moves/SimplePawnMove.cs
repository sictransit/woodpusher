using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Parsing.Moves
{
    public class SimplePawnMove : SimplePieceMove
    {
        public SimplePawnMove(Square square) : base(Piece.Pawn, square)
        {
        }
    }
}
