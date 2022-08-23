using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public struct Piece
    {
        public PieceColour Colour { get; set; }
        public PieceType Type { get; set; }

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
