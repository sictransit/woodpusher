using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class PieceExtensions
    {
        public static Piece ToPiece(this char c)
        {
            var type = char.ToUpperInvariant(c) switch
            {
                'P' => PieceType.Pawn,
                'R' => PieceType.Rook,
                'N' => PieceType.Knight,
                'B' => PieceType.Bishop,
                'Q' => PieceType.Queen,
                'K' => PieceType.King,
                _ => throw new NotImplementedException($"No idea how to parse this piece: '{c}'"),
            };

            var piece = new Piece(char.IsUpper(c) ? PieceColour.White : PieceColour.Black, type);

            return piece;
        }

        public static char ToAlgebraicNotation(this Piece p)
        {
            var c = p.Type switch
            {
                PieceType.Pawn => 'P',
                PieceType.Knight => 'N',
                PieceType.Bishop => 'B',
                PieceType.Rook => 'R',
                PieceType.Queen => 'Q',
                PieceType.King => 'K',                
                _ => throw new NotImplementedException(p.ToString()),
            };

            return p.Colour == PieceColour.White ? c : char.ToLowerInvariant(c);
        }
    }
}
