using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Parsing.Exceptions;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher.Common.Parsing
{
    public static class ForsythEdwardsNotation
    {
        public const string StartingPosition = @"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public static IBoard Parse(string fen)
        {
            if (fen is null)
            {
                throw new ArgumentNullException(nameof(fen));
            }

            var parts = fen.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 6)
            {
                throw new FenParsingException(fen, "parts should be == 6");
            }

            var (white, black) = ParseBoard(parts[0]);

            var activeColor = ParseActiveColour(parts[1]);

            var castling = ParseCastling(parts[2]);

            var enPassantTarget = ParseEnPassantTarget(parts[3]);

            var halfmoveClock = ParseHalfmoveClock(parts[4]);

            var fullmoveNumber = ParseFullmoveNumber(parts[5]);

            var counters = new Counters(activeColor, castling, enPassantTarget?.ToMask() ?? 0, halfmoveClock, fullmoveNumber);

            return new Board(white, black, counters);
        }

        private static (Bitboard, Bitboard) ParseBoard(string s)
        {
            // TODO: regex for validating FEN board setup

            var parts = s.Split('/');

            if (parts.Length != 8)
            {
                throw new FenParsingException(s, "board setup should be eight parts, separated by '/'");
            }

            var rank = 7;

            Bitboard white = new(Piece.White);
            Bitboard black = new(Piece.None);

            foreach (var part in parts)
            {
                var file = 0;

                foreach (var c in part)
                {
                    if (char.IsDigit(c))
                    {
                        file += c - '0';
                    }
                    else
                    {
                        var square = new Square(file++, rank);

                        var piece = c.ToPiece();

                        if (piece.Is(Piece.White))
                        {
                            white = white.Add(piece.SetSquare(square));
                        }
                        else
                        {
                            black = black.Add(piece.SetSquare(square));
                        }
                    }
                }

                rank--;
            }

            return (white, black);
        }

        private static int ParseFullmoveNumber(string s) => int.Parse(s);

        private static int ParseHalfmoveClock(string s) => int.Parse(s);

        private static Square? ParseEnPassantTarget(string s)
        {
            if (s.IsNothing())
            {
                return null;
            }

            if (!s.IsAlgebraicNotation())
            {
                throw new FenParsingException(s, "en passant target should be in algebraic notation or '-'");
            }

            return new Square(s);
        }

        private static Piece ParseActiveColour(string s)
        {
            if (!Regex.IsMatch(s, "^[w|b]$"))
            {
                throw new FenParsingException(s, "active color should be 'w' or 'b'");
            }

            return s.Single() switch
            {
                'w' => Piece.White,
                _ => Piece.None,
            };
        }

        private static Castlings ParseCastling(string s)
        {
            if (!Regex.IsMatch(s, "^K?Q?k?q?$") && !Regex.IsMatch(s, "^-$"))
            {
                throw new FenParsingException(s, "castling should be \"KQkq\" with omitted letters when appropriate, or \"-\" if none");
            }

            Castlings castlings = 0;

            foreach (var c in s)
            {
                switch (c)
                {
                    case 'K':
                        castlings |= Castlings.WhiteKingside;
                        break;
                    case 'Q':
                        castlings |= Castlings.WhiteQueenside;
                        break;
                    case 'k':
                        castlings |= Castlings.BlackKingside;
                        break;
                    case 'q':
                        castlings |= Castlings.BlackQueenside;
                        break;
                }
            }

            return castlings;
        }
    }
}
