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

        public static char ToAlgebraicNotation(this Piece p) => p switch
        {
            Piece.Pawn | Piece.White => 'P',
            Piece.Knight | Piece.White => 'N',
            Piece.Bishop | Piece.White => 'B',
            Piece.Rook | Piece.White => 'R',
            Piece.Queen | Piece.White => 'Q',
            Piece.King | Piece.White => 'K',
            Piece.Pawn | Piece.Black => 'p',
            Piece.Knight | Piece.Black => 'n',
            Piece.Bishop | Piece.Black => 'b',
            Piece.Rook | Piece.Black => 'r',
            Piece.Queen | Piece.Black => 'q',
            Piece.King | Piece.Black => 'k',
            _ => throw new NotImplementedException(p.ToString()),
        };
    }
}
