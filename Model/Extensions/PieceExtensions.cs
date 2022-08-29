using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class PieceExtensions
    {
        public static Piece ToPiece(this char c)
        {
            var piece = char.ToUpperInvariant(c) switch
            {
                'P' => Piece.Pawn,
                'R' => Piece.Rook,
                'N' => Piece.Knight,
                'B' => Piece.Bishop,
                'Q' => Piece.Queen,
                'K' => Piece.King,
                _ => throw new NotImplementedException($"No idea how to parse this piece: '{c}'"),
            };

            piece |= char.IsUpper(c) ? Piece.White : Piece.Black;

            return piece;
        }

        public static char ToChar(this Piece p)
        {
            return ((int)p & Constants.PieceTypeMask) switch
            {
                Constants.Pawn => 'P',
                Constants.Rook => 'R',
                Constants.Knight => 'N',
                Constants.Bishop => 'B',
                Constants.Queen => 'Q',
                Constants.King => 'K',
                _ => throw new NotImplementedException(p.ToString()),
            };
        }

        public static char ToAlgebraicNotation(this Piece p)
        {
            var c = p.ToChar();

            return p.HasFlag(Piece.White) ? c : char.ToLowerInvariant(c);
        }
    }
}
