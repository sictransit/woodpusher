using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model
{
    public class Move
    {
        public Move(Piece piece, ulong target, SpecialMove flags, ulong enPassantTarget = 0, ulong castlingCheckMask = 0, ulong castlingEmptyMask = 0, Piece promotionType = Piece.None)
        {
            Piece = piece;
            Target = target;
            Flags = flags;
            EnPassantTarget = enPassantTarget;
            CastlingCheckMask = castlingCheckMask;
            CastlingEmptySquaresMask = castlingEmptyMask;
            PromotionType = promotionType;

            if (enPassantTarget != 0)
            {
                EnPassantMask = enPassantTarget.AddFileAndRank(0, Piece.Is(Piece.White) ? -1 : 1);
            }
        }

        public Move(Piece piece, Square target, SpecialMove flags = SpecialMove.None) : this(piece, target.ToMask(), flags)
        {
        }

        public Piece Piece { get; }

        public Square GetTarget() => Target.ToSquare();

        public ulong Target { get; }

        public SpecialMove Flags { get; }

        public ulong CastlingCheckMask { get; }

        public ulong CastlingEmptySquaresMask { get; }

        public Piece PromotionType { get; }

        public ulong EnPassantTarget { get; }

        public ulong EnPassantMask { get; }

        public override string ToString()
        {
            return $"{Piece.ToAlgebraicNotation()}{GetTarget()}" + (Flags == SpecialMove.None ? string.Empty : $" ({Flags})" + (Flags.HasFlag(SpecialMove.PawnPromotes) ? $" ={PromotionType}" : string.Empty));
        }
    }
}
