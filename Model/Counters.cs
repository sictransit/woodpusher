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

        public Move LastMove { get; }

        public int Ply { get; }

        public Counters(Piece activeColor, Castlings castlings, ulong enPassantTarget, int halfmoveClock, int fullmoveNumber, Move lastMove, Piece capture)
        {
            ActiveColor = activeColor;
            Castlings = castlings;
            EnPassantTarget = enPassantTarget;
            HalfmoveClock = halfmoveClock;
            FullmoveNumber = fullmoveNumber;
            LastMove = lastMove;
            Capture = capture;
            Ply = fullmoveNumber * 2 - (activeColor == Piece.White ? 2 : 1);
        }

        public static Counters Default => new(Piece.White, Castlings.WhiteKingside | Castlings.WhiteQueenside | Castlings.BlackKingside | Castlings.BlackQueenside, 0, 0, 1, null, Piece.None);
    }
}
