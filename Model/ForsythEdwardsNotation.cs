using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Exceptions;
using SicTransit.Woodpusher.Model.Extensions;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher.Model
{
    public class ForsythEdwardsNotation
    {
        public Board Board { get; }

        public PieceColour ActiveColour { get; }

        public Castlings Castlings { get; }

        public Position? EnPassantTarget { get; }

        public int HalfmoveClock { get; }

        public int FullmoveNumber { get; }

        private ForsythEdwardsNotation(Board board, PieceColour activeColour, Castlings castlings, Position? enPassantTarget, int halfmoveClock, int fullmoveNumber)
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
                        file += c - '1';
                    }
                    else
                    {

                        var piece = c.ToPiece();

                        var position = new Position(file, rank);

                        board.Set(position, piece);

                        file++;
                    }
                }

                rank--;

            }

            return board;
        }

        private static int ParseFullmoveNumber(string s)
        {
            return int.Parse(s);
        }

        private static int ParseHalfmoveClock(string s)
        {
            return int.Parse(s);
        }

        private static Position? ParseEnPassantTarget(string s)
        {
            if (s.IsNothing())
            {
                return null;
            }

            if (!StringExtensions.IsAlgebraicNotation(s))
            {
                throw new FenParsingException(s, "en passant target should be in algebraic notation or '-'");
            }

            return Position.FromAlgebraicNotation(s);
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
