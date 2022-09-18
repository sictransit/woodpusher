using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public class Target
    {
        public Target(Square square, SpecialMove flags = SpecialMove.None) : this(square, flags, null)
        { }

        public Target(Square square, SpecialMove flags, Square? enPassantTarget = null, Square? castlingCheckSquare = null, IEnumerable<Square>? castlingEmptySquares = null, Square? castlingRookSquare = null, PieceType promotionType = PieceType.None)
        {
            Square = square;
            Flags = flags;

            EnPassantTarget = enPassantTarget;

            CastlingCheckSquare = castlingCheckSquare;
            CastlingEmptySquares = castlingEmptySquares ?? Enumerable.Empty<Square>();
            CastlingRookSquare = castlingRookSquare;
            PromotionType = promotionType;
        }

        public Square Square { get; }

        public Square? CastlingCheckSquare { get; }

        public IEnumerable<Square> CastlingEmptySquares { get; }

        public Square? CastlingRookSquare { get; }
        public PieceType PromotionType { get; }
        public Square? EnPassantTarget { get; }

        public SpecialMove Flags { get; }

        public override string ToString()
        {
            return $"{Square}" + (Flags == SpecialMove.None ? string.Empty : $" ({Flags})");
        }

    }
}
