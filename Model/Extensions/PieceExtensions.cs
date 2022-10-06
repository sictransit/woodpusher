using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class PieceExtensions
    {
        public static PieceType ToPieceType(this char c) => c switch
        {
            'P' => PieceType.Pawn,
            'R' => PieceType.Rook,
            'N' => PieceType.Knight,
            'B' => PieceType.Bishop,
            'Q' => PieceType.Queen,
            'K' => PieceType.King,
            _ => throw new NotImplementedException($"No idea how to parse this piece: '{c}'"),
        };


        public static Piece ToPiece(this char c)
        {
            var pieceType = ToPieceType(char.ToUpperInvariant(c));

            var pieceColour = char.IsUpper(c) ? PieceColor.White : PieceColor.Black;

            return new Piece(pieceType, pieceColour);
        }

        public static char ToChar(this PieceType t) => t switch
        {
            PieceType.Pawn => 'P',
            PieceType.Rook => 'R',
            PieceType.Knight => 'N',
            PieceType.Bishop => 'B',
            PieceType.Queen => 'Q',
            PieceType.King => 'K',
            _ => throw new NotImplementedException(t.ToString()),
        };

        public static char ToAlgebraicNotation(this Piece p)
        {
            var c = p.Type.ToChar();

            return p.Color == PieceColor.White ? c : char.ToLowerInvariant(c);
        }

        public static PieceColor OpponentColor(this PieceColor p) => p == PieceColor.White ? PieceColor.Black : PieceColor.White;
    }
}
