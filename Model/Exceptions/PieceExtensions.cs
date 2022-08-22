using SicTransit.Woodpusher.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Model.Exceptions
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

            return new Piece(char.IsUpper(c) ? PieceColour.White : PieceColour.Black, type);
        }
    }
}
