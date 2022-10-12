using SicTransit.Woodpusher.Model.Enums;
using System.Security.Cryptography;

namespace SicTransit.Woodpusher.Model
{
    public class Counters
    {
        public Piece ActiveColor { get; }

        public Castlings WhiteCastlings { get; }

        public Castlings BlackCastlings { get; }

        public ulong EnPassantTarget { get; }

        public int HalfmoveClock { get; }

        public int FullmoveNumber { get; }

        public Counters(Piece activeColor, Castlings whiteCastlings, Castlings blackCastlings, ulong enPassantTarget, int halfmoveClock, int fullmoveNumber)
        {
            ActiveColor = activeColor;
            WhiteCastlings = whiteCastlings;
            BlackCastlings = blackCastlings;
            EnPassantTarget = enPassantTarget;
            HalfmoveClock = halfmoveClock;
            FullmoveNumber = fullmoveNumber;
        }

        public static Counters Default => new(Piece.White, Castlings.Kingside | Castlings.Queenside, Castlings.Kingside | Castlings.Queenside, 0, 0, 0);

        public byte[] Hash
        {
            get
            {
                using var md5 = MD5.Create();

                var bytes = BitConverter.GetBytes((int)ActiveColor).Concat(BitConverter.GetBytes((int)WhiteCastlings)).Concat(BitConverter.GetBytes((int)BlackCastlings)).Concat(BitConverter.GetBytes(EnPassantTarget)).ToArray();

                return md5.ComputeHash(bytes);
            }
        }
    }
}
