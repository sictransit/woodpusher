using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public struct Piece
    {
        public Piece(PieceType type, PieceColor color)
        {
            Type = type;
            Color = color;
        }

        public PieceType Type { get; }

        public PieceColor Color { get; }
    }
}
