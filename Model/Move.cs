using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public struct Move
    {
        public Move(Square square) : this(square, MovementFlags.None)
        {

        }

        public Move(Square square, MovementFlags flags)
        {
            Square = square;
            Flags = flags;
        }

        public Square Square { get; init; }

        public MovementFlags Flags { get; init; }

        public override string ToString()
        {
            return $"{Square}" + (Flags == MovementFlags.None ? string.Empty : $" ({Flags})");
        }

    }
}
