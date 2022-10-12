using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public class Piece
    {
        public Piece(PieceType type, PieceColor color)
        {
            Type = type;
            Color = color;
        }

        public PieceType Type { get; }

        public PieceColor Color { get; }

        public override bool Equals(object? obj)
        {
            return obj is Piece piece &&
                   Type == piece.Type &&
                   Color == piece.Color;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Color);
        }
    }
}
