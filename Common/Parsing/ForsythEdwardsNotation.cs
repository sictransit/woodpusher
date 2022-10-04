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

            var whiteCastling = ParseCastling(parts[2], PieceColor.White);
            var blackCastling = ParseCastling(parts[2], PieceColor.Black);

            var enPassantTarget = ParseEnPassantTarget(parts[3]);

            var halfmoveClock = ParseHalfmoveClock(parts[4]);

            var fullmoveNumber = ParseFullmoveNumber(parts[5]);

            var counters = new Counters(activeColor, whiteCastling, blackCastling, enPassantTarget?.ToMask() ?? 0, halfmoveClock, fullmoveNumber);

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

            Bitboard white = new(PieceColor.White);
            Bitboard black = new(PieceColor.Black);

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

                        if (piece.Color == PieceColor.White)
                        {
                            white = white.Add(piece.Type, square.ToMask());
                        }
                        else
                        {
                            black = black.Add(piece.Type, square.ToMask());
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

        private static PieceColor ParseActiveColour(string s)
        {
            if (!Regex.IsMatch(s, "^[w|b]$"))
            {
                throw new FenParsingException(s, "active colour should be 'w' or 'b'");
            }

            return s.Single() switch
            {
                'w' => PieceColor.White,
                _ => PieceColor.Black,
            };
        }

        private static Castlings ParseCastling(string s, PieceColor pieceColor)
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
                    case 'K' when pieceColor == PieceColor.White:
                        castlings |= Castlings.Kingside;
                        break;
                    case 'Q' when pieceColor == PieceColor.White:
                        castlings |= Castlings.Queenside;
                        break;
                    case 'k' when pieceColor == PieceColor.Black:
                        castlings |= Castlings.Kingside;
                        break;
                    case 'q' when pieceColor == PieceColor.Black:
                        castlings |= Castlings.Queenside;
                        break;
                }
            }

            return castlings;
        }
    }
}
