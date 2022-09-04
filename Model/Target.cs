using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public struct Target
    {
        public Target(Square square) : this(square, SpecialMove.None)
        {

        }

        public Target(Square square, SpecialMove flags)
        {
            Square = square;
            Flags = flags;
        }

        public Square Square { get; init; }

        public SpecialMove Flags { get; init; }

        public override string ToString()
        {
            return $"{Square}" + (Flags == SpecialMove.None ? string.Empty : $" ({Flags})");
        }

    }
}
