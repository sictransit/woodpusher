using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Numerics;
using System.Security.Cryptography;

namespace SicTransit.Woodpusher.Model
{
    public class Bitboard
    {
        // 00 .. 63
        // A1 .. H8

        public Bitboard(Pieces color, ulong pawn = 0, ulong rook = 0, ulong knight = 0, ulong bishop = 0, ulong queen = 0, ulong king = 0)
        {
            this.color = color;
            this.pawn = pawn;
            this.rook = rook;
            this.knight = knight;
            this.bishop = bishop;
            this.queen = queen;
            this.king = king;

            all = pawn | rook | knight | bishop | queen | king;
        }

        private readonly Pieces color;

        private readonly ulong all;

        private readonly ulong pawn;
        private readonly ulong rook;
        private readonly ulong knight;
        private readonly ulong bishop;
        private readonly ulong queen;
        private readonly ulong king;

        public int Phase => BitOperations.PopCount(knight) + BitOperations.PopCount(bishop) + 2 * BitOperations.PopCount(rook) + 4 * BitOperations.PopCount(queen);

        public ulong All => all;

        public ulong King => king;

        public ulong Pawn => pawn;

        public byte[] Hash
        {
            get
            {
                using var md5 = MD5.Create();

                var bytes = BitConverter.GetBytes(pawn).Concat(BitConverter.GetBytes(rook)).Concat(BitConverter.GetBytes(knight)).Concat(BitConverter.GetBytes(bishop)).Concat(BitConverter.GetBytes(queen)).Concat(BitConverter.GetBytes(king)).ToArray();

                return md5.ComputeHash(bytes);
            }
        }

        public bool IsOccupied(ulong mask) => (all & mask) != 0;

        public Bitboard Add(Pieces piece) => Toggle(piece);

        public Bitboard Remove(Pieces piece) => Toggle(piece);

        public Bitboard Move(Pieces piece, ulong to) => Toggle(piece, to);

        private ulong GetBitmap(Pieces pieceType) => pieceType switch
        {
            Pieces.Pawn => pawn,
            Pieces.Knight => knight,
            Pieces.Bishop => bishop,
            Pieces.Rook => rook,
            Pieces.Queen => queen,
            Pieces.King => king,
            _ => throw new ArgumentOutOfRangeException(nameof(pieceType)),
        };

        private static readonly Pieces[] PieceTypes = { Pieces.Pawn, Pieces.Knight, Pieces.Bishop, Pieces.Rook, Pieces.Queen, Pieces.King };

        public IEnumerable<Pieces> GetPositions()
        {
            foreach (var pieceType in PieceTypes)
            {
                foreach (var position in GetPositions(pieceType))
                {
                    yield return position;
                }
            }
        }

        public IEnumerable<Pieces> GetPositions(Pieces type) => GetPositions(type, ulong.MaxValue);

        public IEnumerable<Pieces> GetPositions(Pieces type, ulong mask)
        {
            var bitmap = GetBitmap(type) & mask;

            while (bitmap != 0ul)
            {
                var bit = 1ul << BitOperations.TrailingZeroCount(bitmap);

                yield return (type | color).SetMask(bit);

                bitmap &= ~bit;
            }
        }

        public Pieces Peek(ulong mask)
        {
            if ((all & mask) == 0)
            {
                return Pieces.None;
            }

            if ((pawn & mask) != 0)
            {
                return Pieces.Pawn.SetMask(mask);
            }

            if ((rook & mask) != 0)
            {
                return Pieces.Rook.SetMask(mask);
            }

            if ((knight & mask) != 0)
            {
                return Pieces.Knight.SetMask(mask);
            }

            if ((bishop & mask) != 0)
            {
                return Pieces.Bishop.SetMask(mask);
            }

            if ((queen & mask) != 0)
            {
                return Pieces.Queen.SetMask(mask);
            }

            return Pieces.King.SetMask(mask);
        }

        private Bitboard Toggle(Pieces pieceType, ulong to = 0)
        {
            var mask = pieceType.GetMask() | to;

            if (pieceType.Is(Pieces.Pawn))
            {
                return new Bitboard(color, pawn ^ mask, rook, knight, bishop, queen, king);
            }

            if (pieceType.Is(Pieces.Rook))
            {
                return new Bitboard(color, pawn, rook ^ mask, knight, bishop, queen, king);
            }

            if (pieceType.Is(Pieces.Knight))
            {
                return new Bitboard(color, pawn, rook, knight ^ mask, bishop, queen, king);
            }

            if (pieceType.Is(Pieces.Bishop))
            {
                return new Bitboard(color, pawn, rook, knight, bishop ^ mask, queen, king);
            }

            if (pieceType.Is(Pieces.Queen))
            {
                return new Bitboard(color, pawn, rook, knight, bishop, queen ^ mask, king);
            }

            if (pieceType.Is(Pieces.King))
            {
                return new Bitboard(color, pawn, rook, knight, bishop, queen, king ^ mask);
            }

            throw new ArgumentOutOfRangeException(nameof(pieceType));
        }
    }
}
