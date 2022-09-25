using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public class Move
    {
        public Move(Position position, Square target, SpecialMove flags, Square? enPassantTarget = null, Square? castlingCheckSquare = null, IEnumerable<Square>? castlingEmptySquares = null, Square? castlingRookSquare = null, PieceType promotionType = PieceType.None)
        {
            Position = position;
            Target = target;
            Flags = flags;
            EnPassantTarget = enPassantTarget;

            CastlingCheckSquare = castlingCheckSquare;
            CastlingEmptySquares = castlingEmptySquares ?? Enumerable.Empty<Square>();
            CastlingRookSquare = castlingRookSquare;
            PromotionType = promotionType;
        }

        public Move(Position position, Square target, SpecialMove flags = SpecialMove.None) : this(position, target, flags, null)
        {
        }

        public Position Position { get; }

        public Square Target { get; }
        public SpecialMove Flags { get; }

        public Square? CastlingCheckSquare { get; }

        public IEnumerable<Square> CastlingEmptySquares { get; }

        public Square? CastlingRookSquare { get; }
        public PieceType PromotionType { get; }
        public Square? EnPassantTarget { get; }

        public override string ToString()
        {
            return $"{Position}{Target}" + (Flags == SpecialMove.None ? string.Empty : $" ({Flags})" + (Flags.HasFlag(SpecialMove.Promote) ? $" ={PromotionType}" : string.Empty)); ;
        }
    }
}
