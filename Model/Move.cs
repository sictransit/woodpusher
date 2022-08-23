using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public struct Move
    {
        public Move(Square square) : this(square, MovementFlag.None)
        {

        }

        public Move(Square square, MovementFlag flag)
        {
            Square = square;
            Flag = flag;
        }

        public Square Square { get; init; }

        public MovementFlag Flag { get; init; }

        public override string ToString()
        {
            return $"{Square}" + (Flag == MovementFlag.None ? string.Empty : $" ({Flag})");
        }

    }
}
