﻿using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Extensions;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher.Parsing.Moves
{
    public abstract class PgnMove
    {
        public static PgnMove Parse(string s)
        {
            s = s.Replace("x", string.Empty).Replace("+", string.Empty);

            if (TryParseSimplePawnMove(s, out var simplePawnMove))
            {
                return simplePawnMove!;
            }

            if (TryParseSimplePieceMove(s, out var simplePieceMove))
            {
                return simplePieceMove!;
            }

            if (TryParseCastlingMove(s, out var castlingMove))
            {
                return castlingMove!;
            }

            if (TryParseNamedPieceOnFileMove(s, out var pieceOnFileMove))
            {
                return pieceOnFileMove!;
            }

            if (TryParseFileMove(s, out var fileMove))
            {
                return fileMove!;
            }


            throw new ArgumentException($"unable to parse: [{s}]");
        }

        private static bool TryParseSimplePawnMove(string s, out SimplePawnMove? move)
        {
            move = default;

            Regex regex = new(@"^([a-h][1-8])$"); // e4

            Match match = regex.Match(s);

            if (match.Success)
            {
                move = new SimplePawnMove(Square.FromAlgebraicNotation(match.Captures[0].Value));
            }

            return move != default;
        }

        private static bool TryParseSimplePieceMove(string s, out SimplePieceMove? move)
        {
            move = default;

            Regex regex = new(@"^([PRNBQK])([a-h][1-8])$"); // Nf3

            Match match = regex.Match(s);

            if (match.Success)
            {
                move = new SimplePieceMove(match.Groups[1].Value[0].ToPieceType(), Square.FromAlgebraicNotation(match.Groups[2].Value));
            }

            return move != default;
        }

        private static bool TryParseNamedPieceOnFileMove(string s, out SimplePieceMove? move)
        {
            move = default;

            Regex regex = new(@"^([PRNBQK])([a-h])([a-h][1-8])$"); // Nbd7

            Match match = regex.Match(s);

            if (match.Success)
            {
                move = new PieceOnFileMove(match.Groups[1].Value[0].ToPieceType(), match.Groups[2].Value[0].ToFile(), Square.FromAlgebraicNotation(match.Groups[3].Value));
            }

            return move != default;
        }

        private static bool TryParseFileMove(string s, out FileMove? move)
        {
            move = default;

            Regex regex = new(@"^([a-h])([a-h][1-8])$"); // cb5

            Match match = regex.Match(s);

            if (match.Success)
            {
                move = new FileMove(match.Groups[1].Value[0].ToFile(), Square.FromAlgebraicNotation(match.Groups[2].Value));
            }

            return move != default;
        }


        private static bool TryParseCastlingMove(string s, out CastlingMove? move)
        {
            move = default;

            Regex regex = new(@"^(O\-O)(\-O)?$"); // O-O[-O]

            var match = regex.Match(s);

            if (match.Success)
            {
                if (string.IsNullOrEmpty(match.Groups[2].Value))
                {
                    move = new CastlingKingMove();
                }
                else
                {
                    move = new CastlingQueenMove();
                }
            }

            return move != default;
        }

        protected abstract void Apply(IEngine engine);
    }
}