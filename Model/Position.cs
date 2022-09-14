using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model
{
    public struct Position
    {
        public Position(Piece piece, Square square)
        {
            Piece = piece;
            Square = square;
        }

        public Piece Piece { get; }

        public Square Square { get; }

        public override string ToString()
        {
            return $"{Piece.ToChar()}{Square}";
        }
    }
}
