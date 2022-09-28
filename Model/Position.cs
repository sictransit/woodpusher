using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model
{
    public class Position
    {
        public Position(Piece piece, Square square)
        {
            Piece = piece;
            Square = square;
        }

        public Piece Piece { get; }

        public Square Square { get; }

        public override bool Equals(object? obj)
        {
            return obj is Position position &&
                   EqualityComparer<Piece>.Default.Equals(Piece, position.Piece) &&
                   EqualityComparer<Square>.Default.Equals(Square, position.Square);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Piece, Square);
        }

        public override string ToString()
        {
            return $"{Piece.Type.ToChar()}{Square}";
        }
    }
}
