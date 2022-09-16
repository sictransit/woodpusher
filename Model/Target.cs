using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public class Target
    {
        public Target(Square square, SpecialMove flags = SpecialMove.None) : this(square, flags, null)
        { }

        public Target(Square square, SpecialMove flags, Square? referenceSquare)
        {
            Square = square;
            Flags = flags;
            ReferenceSquare = referenceSquare;
        }

        public Square Square { get; init; }

        public Square? ReferenceSquare { get; init; }

        public SpecialMove Flags { get; init; }

        public override string ToString()
        {
            return $"{Square}" + (Flags == SpecialMove.None ? string.Empty : $" ({Flags})");
        }

    }
}
