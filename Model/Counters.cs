using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public struct Counters
    {
        public PieceColour ActiveColour { get; init; }

        public Castlings WhiteCastlings { get; init; }

        public Castlings BlackCastlings { get; init; }

        public Square? EnPassantTarget { get; init; }

        public int HalfmoveClock { get; init; }

        public int FullmoveNumber { get; init; }

        public Counters(PieceColour activeColour, Castlings whiteCastlings, Castlings blackCastlings, Square? enPassantTarget, int halfmoveClock, int fullmoveNumber)
        {
            ActiveColour = activeColour;
            WhiteCastlings = whiteCastlings;
            BlackCastlings = blackCastlings;
            EnPassantTarget = enPassantTarget;
            HalfmoveClock = halfmoveClock;
            FullmoveNumber = fullmoveNumber;
        }

        public static Counters Default => new(PieceColour.White, Castlings.Kingside | Castlings.Queenside, Castlings.Kingside | Castlings.Queenside, null, 0, 0);
    }
}
