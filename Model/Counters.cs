using SicTransit.Woodpusher.Model.Enums;
using System.Security.Cryptography;

namespace SicTransit.Woodpusher.Model
{
    public class Counters
    {
        public Piece ActiveColor { get; }

        public Castlings Castlings { get; }

        public ulong EnPassantTarget { get; }

        public int HalfmoveClock { get; }

        public int FullmoveNumber { get; }

        public Counters(Piece activeColor, Castlings castlings, ulong enPassantTarget, int halfmoveClock, int fullmoveNumber)
        {
            ActiveColor = activeColor;
            Castlings = castlings;
            EnPassantTarget = enPassantTarget;
            HalfmoveClock = halfmoveClock;
            FullmoveNumber = fullmoveNumber;
        }

        public static Counters Default => new(Piece.White, Castlings.WhiteKingside | Castlings.WhiteQueenside | Castlings.BlackKingside | Castlings.BlackQueenside, 0, 0, 0);

        public IEnumerable<byte> Hash => BitConverter.GetBytes((int)ActiveColor).Concat(BitConverter.GetBytes((int)Castlings)).Concat(BitConverter.GetBytes(EnPassantTarget));
    }
}
