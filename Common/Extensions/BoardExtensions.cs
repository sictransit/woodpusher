using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Parsing;
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

            var pieces = b.GetPieces(Piece.White).Concat(b.GetPieces(Piece.None)).ToList();

            for (var rank = 7; rank >= 0; rank--)
            {
                sb.Append($"{rank + 1} ");
                for (var file = 0; file < 8; file++)
                {
                    var square = new Square(file, rank);

                    var piece = pieces.SingleOrDefault(p => p.GetSquare().Equals(square));

                    var c = piece != default ? piece.ToAlgebraicNotation() : '·';

                    sb.Append($"{c} ");
                }

                sb.AppendLine();
            }

            sb.Append("  ");

            for (var i = 0; i < 8; i++)
            {
                sb.Append($"{(char)('a' + i)} ");
            }

            sb.AppendLine();

            sb.AppendLine($"Hash: {b.Hash}");
            sb.AppendLine($"FEN: {ForsythEdwardsNotation.Export(b)}");

            return sb.ToString();
        }

        public static ulong Perft(this IBoard board, int depth)
        {
            ulong count = 0;

            Parallel.ForEach(board.GetLegalMoves(), m =>
            {
                Interlocked.Add(ref count, board.Play(m).ParallelPerft(depth - 1));
            });

            return count;
        }

        private static ulong ParallelPerft(this IBoard board, int depth)
        {
            if (depth <= 1)
            {
                return 1;
            }

            ulong count = 0;

            foreach (var move in board.GetLegalMoves())
            {
                count += board.Play(move).ParallelPerft(depth - 1);
            }

            return count;
        }
    }
}
