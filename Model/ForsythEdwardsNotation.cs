using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Exceptions;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher.Model
{
    public class ForsythEdwardsNotation
    {
        public PieceColour ActiveColour { get; }
        public Board Board { get; }
        public PieceColour ActiveColour1 { get; }
        public Castlings Castlings { get; }

        private ForsythEdwardsNotation(Board board, PieceColour activeColour, Castlings castlings)
        {
            Board = board;
            ActiveColour1 = activeColour;
            Castlings = castlings;
        }

        public static ForsythEdwardsNotation Parse(string fen)
        {
            if (fen is null)
            {
                throw new ArgumentNullException(nameof(fen));
            }

            Board board = new Board();

            var parts = fen.Split(' ');

            if (parts.Length != 6)
            {
                throw new FenParsingException(fen, "parts should be == 6");
            }

            var colour = ParseActiveColour(parts[1]);

            var castling = ParseCastling(parts[2]);

            return new ForsythEdwardsNotation(board, colour, castling);
        }

        private static PieceColour ParseActiveColour(string activeColourPart)
        {
            if (!Regex.IsMatch(activeColourPart, "^[w|b]$"))
            {
                throw new FenParsingException(activeColourPart, "active colour should be 'w' or 'b'");
            }

            return activeColourPart.Single() switch
            {
                'w' => PieceColour.White,
                _ => PieceColour.Black,
            };
        }

        private static Castlings ParseCastling(string castlingPart)
        {
            Castlings castlings = 0;

            castlings |= Castlings.WhiteKingside;

            return castlings;

        }
    }
}
