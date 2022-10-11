using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Text;

namespace SicTransit.Woodpusher.Common.Extensions
{
    public static class BoardExtensions
    {
        public static string PrettyPrint(this IBoard b)
        {
            var sb = new StringBuilder();

            var positions = b.GetPositions(PieceColor.White).Concat(b.GetPositions(PieceColor.Black));

            for (var rank = 7; rank >= 0; rank--)
            {
                sb.Append($"{rank + 1} ");
                for (var file = 0; file < 8; file++)
                {
                    var square = new Square(file, rank);

                    var position = positions.SingleOrDefault(p => p.Square.Equals(square));

                    var c = position != null ? position.Piece.ToAlgebraicNotation() : ' ';

                    sb.Append($"{c} ");
                }

                sb.AppendLine();
            }

            sb.Append("  ");

            for (var i = 0; i < 8; i++)
            {
                sb.Append($"{(char)('a' + i)} ");
            }

            return sb.ToString();
        }

        public static ulong Perft(this IBoard board, int depth)
        {
            if (depth <= 1)
            {
                return 1;
            }

            ulong count = 0;

            foreach (var move in board.GetLegalMoves())
            {
                count += Perft(board.PlayMove(move), depth - 1);
            }

            return count;
        }
    }
}
