using woodpusher.model.enums;

namespace woodpusher.model
{
    public struct Piece
    {
        public PieceColour Colour;
        public PieceType Type;

        public Piece(PieceColour colour, PieceType type)
        {
            Colour = colour;
            Type = type;
        }
    }
}
