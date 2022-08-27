using SicTransit.Woodpusher.Model.Enums;

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
    }
}
