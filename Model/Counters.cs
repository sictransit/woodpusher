using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public class Counters
    {
        public Piece ActiveColor { get; }

        public Castlings Castlings { get; }

        public ulong EnPassantTarget { get; }

        public int HalfmoveClock { get; }

        public int FullmoveNumber { get; }

        public Piece Capture { get; }

        public int Ply => FullmoveNumber * 2 - (ActiveColor == Piece.White ? 2 : 1);

        public Counters(Piece activeColor, Castlings castlings, ulong enPassantTarget, int halfmoveClock, int fullmoveNumber, Piece capture)
        {
            ActiveColor = activeColor;
            Castlings = castlings;
            EnPassantTarget = enPassantTarget;
            HalfmoveClock = halfmoveClock;
            FullmoveNumber = fullmoveNumber;
            Capture = capture;
        }

        public static Counters Default => new(Piece.White, Castlings.WhiteKingside | Castlings.WhiteQueenside | Castlings.BlackKingside | Castlings.BlackQueenside, 0, 0, 1, Piece.None);
    }
}
