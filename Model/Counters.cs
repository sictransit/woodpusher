using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public struct Counters
    {
        public PieceColour ActiveColour { get; init; }

        public Castlings Castlings { get; init; }

        public Square? EnPassantTarget { get; init; }

        public int HalfmoveClock { get; init; }

        public int FullmoveNumber { get; init; }

        public Counters(PieceColour activeColour, Castlings castlings, Square? enPassantTarget, int halfmoveClock, int fullmoveNumber)
        {
            ActiveColour = activeColour;
            Castlings = castlings;
            EnPassantTarget = enPassantTarget;
            HalfmoveClock = halfmoveClock;
            FullmoveNumber = fullmoveNumber;
        }

        public static Counters Default => new Counters(PieceColour.White, Castlings.WhiteKingside | Castlings.WhiteQueenside | Castlings.BlackKingside | Castlings.BlackQueenside, null, 0, 0);
    }
}
