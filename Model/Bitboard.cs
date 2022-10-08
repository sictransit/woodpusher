using SicTransit.Woodpusher.Model.Enums;
using System.Numerics;
using System.Security.Cryptography;

namespace SicTransit.Woodpusher.Model
{
    public class Bitboard
    {
        // 00 .. 63
        // A1 .. H8

        public Bitboard(PieceColor color, ulong pawn = 0, ulong rook = 0, ulong knight = 0, ulong bishop = 0, ulong queen = 0, ulong king = 0)
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

        private readonly PieceColor color;

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

        public Bitboard Add(PieceType pieceType, ulong mask) => Toggle(pieceType, mask);

        public Bitboard Remove(PieceType pieceType, ulong mask) => Toggle(pieceType, mask);

        public Bitboard Move(PieceType pieceType, ulong from, ulong to) => Toggle(pieceType, from | to);

        private ulong GetBitmap(PieceType pieceType) => pieceType switch
        {
            PieceType.Pawn => pawn,
            PieceType.Knight => knight,
            PieceType.Bishop => bishop,
            PieceType.Rook => rook,
            PieceType.Queen => queen,
            PieceType.King => king,
            _ => throw new ArgumentOutOfRangeException(nameof(pieceType)),
        };

        public IEnumerable<Position> GetPositions()
        {
            foreach (var pieceType in new[] { PieceType.Pawn, PieceType.Knight, PieceType.Bishop, PieceType.Rook, PieceType.Queen, PieceType.King })
            {
                foreach (var position in GetPositions(pieceType))
                {
                    yield return position;
                }
            }
        }

        public IEnumerable<Position> GetPositions(PieceType type) => GetPositions(type, ulong.MaxValue);

        public IEnumerable<Position> GetPositions(PieceType type, ulong mask)
        {
            var bitmap = GetBitmap(type) & mask;

            while (bitmap != 0ul)
            {
                var bit = 1ul << BitOperations.TrailingZeroCount(bitmap);

                yield return new Position(new Piece(type, color), bit);

                bitmap &= ~bit;
            }
        }

        public PieceType Peek(ulong mask)
        {
            if ((all & mask) == 0)
            {
                return PieceType.None;
            }

            if ((pawn & mask) != 0)
            {
                return PieceType.Pawn;
            }

            if ((rook & mask) != 0)
            {
                return PieceType.Rook;
            }

            if ((knight & mask) != 0)
            {
                return PieceType.Knight;
            }

            if ((bishop & mask) != 0)
            {
                return PieceType.Bishop;
            }

            if ((queen & mask) != 0)
            {
                return PieceType.Queen;
            }

            return PieceType.King;
        }

        private Bitboard Toggle(PieceType pieceType, ulong mask) => pieceType switch
        {
            PieceType.Pawn => new Bitboard(color, pawn ^ mask, rook, knight, bishop, queen, king),
            PieceType.Rook => new Bitboard(color, pawn, rook ^ mask, knight, bishop, queen, king),
            PieceType.Knight => new Bitboard(color, pawn, rook, knight ^ mask, bishop, queen, king),
            PieceType.Bishop => new Bitboard(color, pawn, rook, knight, bishop ^ mask, queen, king),
            PieceType.Queen => new Bitboard(color, pawn, rook, knight, bishop, queen ^ mask, king),
            PieceType.King => new Bitboard(color, pawn, rook, knight, bishop, queen, king ^ mask),
            _ => throw new ArgumentOutOfRangeException(nameof(pieceType)),
        };
    }
}
