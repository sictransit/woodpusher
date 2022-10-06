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
            Pawn = pawn;
            Rook = rook;
            Knight = knight;
            Bishop = bishop;
            Queen = queen;
            King = king;

            All = Pawn | Rook | Knight | Bishop | Queen | King;
        }

        private readonly PieceColor color;

        public ulong All { get; }

        public ulong Pawn { get; }
        public ulong Rook { get; }
        public ulong Knight { get; }
        public ulong Bishop { get; }
        public ulong Queen { get; }
        public ulong King { get; }

        public int Phase
        {
            get
            {
                return BitOperations.PopCount(Knight) + BitOperations.PopCount(Bishop) + 2 * BitOperations.PopCount(Rook) + 4 * BitOperations.PopCount(Queen);
            }
        }

        public byte[] Hash
        {
            get
            {
                using var md5 = MD5.Create();

                var bytes = BitConverter.GetBytes(Pawn).Concat(BitConverter.GetBytes(Rook)).Concat(BitConverter.GetBytes(Knight)).Concat(BitConverter.GetBytes(Bishop)).Concat(BitConverter.GetBytes(Queen)).Concat(BitConverter.GetBytes(King)).ToArray();

                return md5.ComputeHash(bytes);
            }
        }

        public bool IsOccupied(ulong mask) => (All & mask) != 0;

        public Bitboard Add(PieceType pieceType, ulong mask)
        {
            if ((All & mask) != 0)
            {
                throw new InvalidOperationException("That square is already occupied.");
            }

            return Toggle(pieceType, mask);
        }

        public Bitboard Remove(PieceType pieceType, ulong mask)
        {
            if ((All & mask) == 0)
            {
                throw new InvalidOperationException("There is no piece on that square.");
            }

            return Toggle(pieceType, mask);
        }

        public Bitboard Move(PieceType pieceType, ulong from, ulong to)
        {
            return Toggle(pieceType, from | to);
        }

        private ulong GetBitmap(PieceType pieceType) => pieceType switch
        {
            PieceType.Pawn => Pawn,
            PieceType.Knight => Knight,
            PieceType.Bishop => Bishop,
            PieceType.Rook => Rook,
            PieceType.Queen => Queen,
            PieceType.King => King,
            _ => throw new ArgumentOutOfRangeException(nameof(pieceType)),
        };

        public IEnumerable<Position> GetPieces()
        {
            foreach (var pieceType in new[] { PieceType.Pawn, PieceType.Knight, PieceType.Bishop, PieceType.Rook, PieceType.Queen, PieceType.King })
            {
                foreach (var position in GetPieces(pieceType))
                {
                    yield return position;
                }
            }
        }

        public IEnumerable<Position> GetPieces(PieceType type) => GetPieces(type, ulong.MaxValue);

        public IEnumerable<Position> GetPieces(PieceType type, ulong mask)
        {
            var bitmap = GetBitmap(type) & mask;

            while (bitmap != 0ul)
            {
                var bit = 1ul << BitOperations.TrailingZeroCount(bitmap);

                bitmap &= ~bit;

                yield return new Position(new Piece(type, color), bit);
            }
        }

        public PieceType Peek(ulong mask)
        {
            if ((All & mask) == 0)
            {
                return PieceType.None;
            }

            if ((Pawn & mask) != 0)
            {
                return PieceType.Pawn;
            }

            if ((Rook & mask) != 0)
            {
                return PieceType.Rook;
            }

            if ((Knight & mask) != 0)
            {
                return PieceType.Knight;
            }

            if ((Bishop & mask) != 0)
            {
                return PieceType.Bishop;
            }

            if ((Queen & mask) != 0)
            {
                return PieceType.Queen;
            }

            return PieceType.King;
        }

        private Bitboard Toggle(PieceType pieceType, ulong mask) => pieceType switch
        {
            PieceType.Pawn => new Bitboard(color, Pawn ^ mask, Rook, Knight, Bishop, Queen, King),
            PieceType.Rook => new Bitboard(color, Pawn, Rook ^ mask, Knight, Bishop, Queen, King),
            PieceType.Knight => new Bitboard(color, Pawn, Rook, Knight ^ mask, Bishop, Queen, King),
            PieceType.Bishop => new Bitboard(color, Pawn, Rook, Knight, Bishop ^ mask, Queen, King),
            PieceType.Queen => new Bitboard(color, Pawn, Rook, Knight, Bishop, Queen ^ mask, King),
            PieceType.King => new Bitboard(color, Pawn, Rook, Knight, Bishop, Queen, King ^ mask),
            _ => throw new ArgumentOutOfRangeException(nameof(pieceType)),
        };
    }
}
