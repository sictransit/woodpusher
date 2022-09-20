using System.Text;

namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class BoardExtensions
    {
        public static string PrettyPrint(this Board b)
        {
            var sb = new StringBuilder();

            for (var rank = 7; rank >= 0; rank--)
            {
                sb.Append($"{rank + 1} ");
                for (var file = 0; file < 8; file++)
                {
                    var square = new Square(file, rank);

                    var c = b.IsOccupied(square) ? b.Get(square).ToAlgebraicNotation() : ' ';

                    sb.Append($"{c} ");
                }

                sb.AppendLine();
            }

            sb.Append("  ");

            for (var i = 0; i < 8; i++)
            {
                sb.Append($"{(char)('A' + i)} ");
            }

            return sb.ToString();
        }

        public static ulong Perft(this Board board, int depth)
        {
            if (depth <= 1)
            {
                return 1;
            }

            ulong count = 0;

            foreach (var move in board.GetValidMoves())
            {
                count += Perft(board.Play(move), depth - 1);
            }

            return count;
        }
    }
}
