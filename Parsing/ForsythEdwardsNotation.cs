using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Parsing.Exceptions;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher.Parsing
{
    public static class ForsythEdwardsNotation
    {
        public const string StartingPosition = @"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public static Board Parse(string fen)
        {
            if (fen is null)
            {
                throw new ArgumentNullException(nameof(fen));
            }

            var parts = fen.Split(' ');

            if (parts.Length != 6)
            {
                throw new FenParsingException(fen, "parts should be == 6");
            }

            var board = ParseBoard(parts[0]);

            var activeColour = ParseActiveColour(parts[1]);

            var whiteCastling = ParseCastling(parts[2], PieceColour.White);
            var blackCastling = ParseCastling(parts[2], PieceColour.Black);

            var enPassantTarget = ParseEnPassantTarget(parts[3]);

            var halfmoveClock = ParseHalfmoveClock(parts[4]);

            var fullmoveNumber = ParseFullmoveNumber(parts[5]);

            var counters = new Counters(activeColour, whiteCastling, blackCastling, enPassantTarget, halfmoveClock, fullmoveNumber);

            return new Board(board, counters);
        }

        private static Board ParseBoard(string s)
        {
            // TODO: regex for validating FEN board setup

            var parts = s.Split('/');

            if (parts.Length != 8)
            {
                throw new FenParsingException(s, "board setup should be eight parts, separated by '/'");
            }

            var board = new Board();

            var rank = 7;

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

                        board = board.AddPiece(square, c.ToPiece());
                    }
                }

                rank--;
            }

            return board;
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

        private static PieceColour ParseActiveColour(string s)
        {
            if (!Regex.IsMatch(s, "^[w|b]$"))
            {
                throw new FenParsingException(s, "active colour should be 'w' or 'b'");
            }

            return s.Single() switch
            {
                'w' => PieceColour.White,
                _ => PieceColour.Black,
            };
        }

        private static Castlings ParseCastling(string s, PieceColour pieceColour)
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
                    case 'K' when pieceColour == PieceColour.White:
                        castlings |= Castlings.Kingside;
                        break;
                    case 'Q' when pieceColour == PieceColour.White:
                        castlings |= Castlings.Queenside;
                        break;
                    case 'k' when pieceColour == PieceColour.Black:
                        castlings |= Castlings.Kingside;
                        break;
                    case 'q' when pieceColour == PieceColour.Black:
                        castlings |= Castlings.Queenside;
                        break;
                }
            }

            return castlings;
        }
    }
}
