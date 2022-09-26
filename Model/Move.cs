using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model
{
    public class Move
    {
        public Move(Position position, Square target, SpecialMove flags, Square? enPassantTarget = null, Square? castlingCheckSquare = null, IEnumerable<Square>? castlingEmptySquares = null, PieceType promotionType = PieceType.None)
        {
            Position = position;
            Target = target;
            TargetMask = target.ToMask();
            Flags = flags;
            EnPassantTarget = enPassantTarget;

            CastlingCheckMask= castlingCheckSquare.HasValue ? castlingCheckSquare.Value.ToMask() : 0;
            CastlingEmptySquaresMask = castlingEmptySquares != null ? castlingEmptySquares.ToMask() : 0;
            PromotionType = promotionType;
        }

        public Move(Position position, Square target, SpecialMove flags = SpecialMove.None) : this(position, target, flags, null)
        {
        }

        public Position Position { get; }

        public Square Target { get; }

        public ulong TargetMask { get; }

        public SpecialMove Flags { get; }

        public ulong CastlingCheckMask { get; }

        public ulong CastlingEmptySquaresMask { get; }

        public PieceType PromotionType { get; }
        public Square? EnPassantTarget { get; }

        public override string ToString()
        {
            return $"{Position}{Target}" + (Flags == SpecialMove.None ? string.Empty : $" ({Flags})" + (Flags.HasFlag(SpecialMove.Promote) ? $" ={PromotionType}" : string.Empty)); ;
        }
    }
}
