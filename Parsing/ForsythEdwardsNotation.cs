using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Parsing.Exceptions;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher.Parsing
{
    public class ForsythEdwardsNotation
    {
        public Board Board { get; }

        public Piece ActiveColour { get; }

        public Castlings Castlings { get; }

        public Square? EnPassantTarget { get; }

        public int HalfmoveClock { get; }

        public int FullmoveNumber { get; }

        public const string StartingPosition = @"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        private ForsythEdwardsNotation(Board board, Piece activeColour, Castlings castlings, Square? enPassantTarget, int halfmoveClock, int fullmoveNumber)
        {
            Board = board;
            ActiveColour = activeColour;
            Castlings = castlings;
            EnPassantTarget = enPassantTarget;
            HalfmoveClock = halfmoveClock;
            FullmoveNumber = fullmoveNumber;
        }

        public static ForsythEdwardsNotation Parse(string fen)
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

            var castling = ParseCastling(parts[2]);

            var enPassantTarget = ParseEnPassantTarget(parts[3]);

            var halfmoveClock = ParseHalfmoveClock(parts[4]);

            var fullmoveNumber = ParseFullmoveNumber(parts[5]);

            return new ForsythEdwardsNotation(board, activeColour, castling, enPassantTarget, halfmoveClock, fullmoveNumber);
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

            for (int p = 0; p < parts.Length; p++)
            {
                var file = 0;

                foreach (var c in parts[p])
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

            if (!StringExtensions.IsAlgebraicNotation(s))
            {
                throw new FenParsingException(s, "en passant target should be in algebraic notation or '-'");
            }

            return Square.FromAlgebraicNotation(s);
        }

        private static Piece ParseActiveColour(string s)
        {
            if (!Regex.IsMatch(s, "^[w|b]$"))
            {
                throw new FenParsingException(s, "active colour should be 'w' or 'b'");
            }

            return s.Single() switch
            {
                'w' => Piece.White,
                _ => Piece.Black,
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
