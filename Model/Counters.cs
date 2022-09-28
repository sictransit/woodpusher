using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public struct Counters
    {
        public PieceColor ActiveColor { get; }

        public Castlings WhiteCastlings { get; }

        public Castlings BlackCastlings { get; }

        public ulong EnPassantTarget { get; }

        public int HalfmoveClock { get; }

        public int FullmoveNumber { get; }

        public Counters(PieceColor activeColor, Castlings whiteCastlings, Castlings blackCastlings, ulong enPassantTarget, int halfmoveClock, int fullmoveNumber)
        {
            ActiveColor = activeColor;
            WhiteCastlings = whiteCastlings;
            BlackCastlings = blackCastlings;
            EnPassantTarget = enPassantTarget;
            HalfmoveClock = halfmoveClock;
            FullmoveNumber = fullmoveNumber;
        }

        public static Counters Default => new(PieceColor.White, Castlings.Kingside | Castlings.Queenside, Castlings.Kingside | Castlings.Queenside, 0, 0, 0);
    }
}
