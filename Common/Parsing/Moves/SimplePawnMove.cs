using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Parsing.Moves
{
    public class SimplePawnMove : SimplePieceMove
    {
        public SimplePawnMove(string raw, Square square) : base(raw, Pieces.Pawn, square)
        {
        }
    }
}
