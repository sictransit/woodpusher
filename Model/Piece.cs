using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public struct Piece
    {
        public Piece(PieceType type, PieceColour colour)
        {
            Type = type;
            Colour = colour;
        }

        public PieceType Type { get; init; }

        public PieceColour Colour { get; init; }
    }
}
