using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
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

        public override string ToString()
        {
            return $"{Colour} {Type}";
        }
    }
}
