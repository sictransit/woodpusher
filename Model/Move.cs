using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model
{
    public class Move
    {
        public Move(Position position, ulong target, SpecialMove flags, ulong enPassantTarget = 0, ulong castlingCheckMask = 0, ulong castlingEmptyMask = 0, PieceType promotionType = PieceType.None)
        {
            Position = position;
            Target = target;
            Flags = flags;
            EnPassantTarget = enPassantTarget;
            CastlingCheckMask = castlingCheckMask;
            CastlingEmptySquaresMask = castlingEmptyMask;
            PromotionType = promotionType;

            if (enPassantTarget != 0)
            {
                EnPassantMask = enPassantTarget.AddFileAndRank(0, Position.Piece.Color == PieceColor.White ? -1 : 1);
            }
        }

        public Move(Position position, Square target, SpecialMove flags = SpecialMove.None) : this(position, target.ToMask(), flags)
        {
        }

        public Position Position { get; }

        public Square GetTarget() => Target.ToSquare();

        public ulong Target { get; }

        public SpecialMove Flags { get; }

        public ulong CastlingCheckMask { get; }

        public ulong CastlingEmptySquaresMask { get; }

        public PieceType PromotionType { get; }

        public ulong EnPassantTarget { get; }

        public ulong EnPassantMask { get; }

        public override string ToString()
        {
            return $"{Position}{GetTarget()}" + (Flags == SpecialMove.None ? string.Empty : $" ({Flags})" + (Flags.HasFlag(SpecialMove.Promote) ? $" ={PromotionType}" : string.Empty));
        }
    }
}
