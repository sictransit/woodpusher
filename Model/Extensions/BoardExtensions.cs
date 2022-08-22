using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class BoardExtensions
    {
        public static string PrettyPrint(this Board b)
        {
            var sb = new StringBuilder();

            for (int rank = 7; rank > 0; rank--)
            {
                for (int file = 0; file < 8; file++)
                {
                    var piece = b.Get(new Position(file, rank));
                    var c = piece.ToAlgebraicNotation();

                    sb.Append(c);
                    
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
