﻿using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using System.Text;

namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class BoardExtensions
    {
        public static string PrettyPrint(this Board b)
        {
            var sb = new StringBuilder();

            for (int rank = 7; rank >= 0; rank--)
            {
                sb.Append($"{rank + 1} ");
                for (int file = 0; file < 8; file++)
                {
                    var square = new Square(file, rank);
                    
                    var c = b.IsOccupied(square) ? b.Get(square).ToAlgebraicNotation() : ' ';

                    sb.Append($"{c} ");
                }

                sb.AppendLine();
            }

            sb.Append("  ");

            for (int i = 0; i < 8; i++)
            {
                sb.Append($"{(char)('A' + i)} ");
            }

            return sb.ToString();
        }
    }
}
