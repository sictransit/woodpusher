using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public class Target
    {
        public Target(Square square, SpecialMove flags = SpecialMove.None) : this(square, flags, null)
        { }

        public Target(Square square, SpecialMove flags, Square? enPassantTarget = null, Square? castlingCheckSquare = null, Square? castlingEmptySquare = null, Square? castlingRookSquare = null)
        {
            Square = square;
            Flags = flags;

            EnPassantTarget = enPassantTarget;

            CastlingCheckSquare = castlingCheckSquare;
            CastlingEmptySquare = castlingEmptySquare;
            CastlingRookSquare = castlingRookSquare;
        }

        public Square Square { get; }

        public Square? CastlingCheckSquare { get; }

        public Square? CastlingEmptySquare { get; }

        public Square? CastlingRookSquare { get; }

        public Square? EnPassantTarget { get; }

        public SpecialMove Flags { get; }

        public override string ToString()
        {
            return $"{Square}" + (Flags == SpecialMove.None ? string.Empty : $" ({Flags})");
        }

    }
}
