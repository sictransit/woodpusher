using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Numerics;

namespace SicTransit.Woodpusher.Model
{
    public class Bitboard
    {
        // 00 .. 63
        // A1 .. H8

        public Bitboard(Piece color, ulong pawn = 0, ulong rook = 0, ulong knight = 0, ulong bishop = 0, ulong queen = 0, ulong king = 0)
        {
            Color = color;
            Pawn = pawn;
            Rook = rook;
            Knight = knight;
            Bishop = bishop;
            Queen = queen;
            King = king;

            All = pawn | rook | knight | bishop | queen | king;
        }

        public Piece Color { get; }

        public ulong Pawn { get; }
        public ulong Rook { get; }
        public ulong Knight { get; }
        public ulong Bishop { get; }
        public ulong Queen { get; }
        public ulong King { get; }
        public ulong All { get; }


        public int Phase => BitOperations.PopCount(Knight) + BitOperations.PopCount(Bishop) + 2 * BitOperations.PopCount(Rook) + 4 * BitOperations.PopCount(Queen);



        public bool IsOccupied(ulong mask) => (All & mask) != 0;

        public Bitboard Add(Piece piece) => Toggle(piece);

        public Bitboard Remove(Piece piece) => Toggle(piece);

        public Bitboard Move(Piece piece, ulong to) => Toggle(piece, to);

        private ulong GetBitmap(Piece pieceType) => pieceType switch
        {
            Piece.Pawn => Pawn,
            Piece.Knight => Knight,
            Piece.Bishop => Bishop,
            Piece.Rook => Rook,
            Piece.Queen => Queen,
            Piece.King => King,
            _ => throw new ArgumentOutOfRangeException(nameof(pieceType)),
        };

        public IEnumerable<Piece> GetPieces()
        {
            foreach (var pieceType in PieceExtensions.Types)
            {
                foreach (var piece in GetPieces(pieceType))
                {
                    yield return piece;
                }
            }
        }

        private IEnumerable<Piece> GetPieces(Piece type) => GetPieces(type, ulong.MaxValue);

        public IEnumerable<ulong> GetPositions(Piece type, ulong mask)
        {
            var bitmap = GetBitmap(type) & mask;

            while (bitmap != 0ul)
            {
                var bit = 1ul << BitOperations.TrailingZeroCount(bitmap);

                yield return bit;

                bitmap &= ~bit;
            }
        }

        public IEnumerable<Piece> GetPieces(Piece type, ulong mask)
        {
            var bitmap = GetBitmap(type) & mask;

            while (bitmap != 0ul)
            {
                var bit = 1ul << BitOperations.TrailingZeroCount(bitmap);

                yield return (type | Color).SetMask(bit);

                bitmap &= ~bit;
            }
        }

        public Piece Peek(ulong mask)
        {
            if ((All & mask) == 0)
            {
                return Piece.None;
            }

            if ((Pawn & mask) != 0)
            {
                return Color | Piece.Pawn.SetMask(mask);
            }

            if ((Rook & mask) != 0)
            {
                return Color | Piece.Rook.SetMask(mask);
            }

            if ((Knight & mask) != 0)
            {
                return Color | Piece.Knight.SetMask(mask);
            }

            if ((Bishop & mask) != 0)
            {
                return Color | Piece.Bishop.SetMask(mask);
            }

            if ((Queen & mask) != 0)
            {
                return Color | Piece.Queen.SetMask(mask);
            }

            return Color | Piece.King.SetMask(mask);
        }

        private Bitboard Toggle(Piece pieceType, ulong to = 0)
        {
            var mask = pieceType.GetMask() | to;

            if (pieceType.Is(Piece.Pawn))
            {
                return new Bitboard(Color, Pawn ^ mask, Rook, Knight, Bishop, Queen, King);
            }

            if (pieceType.Is(Piece.Rook))
            {
                return new Bitboard(Color, Pawn, Rook ^ mask, Knight, Bishop, Queen, King);
            }

            if (pieceType.Is(Piece.Knight))
            {
                return new Bitboard(Color, Pawn, Rook, Knight ^ mask, Bishop, Queen, King);
            }

            if (pieceType.Is(Piece.Bishop))
            {
                return new Bitboard(Color, Pawn, Rook, Knight, Bishop ^ mask, Queen, King);
            }

            if (pieceType.Is(Piece.Queen))
            {
                return new Bitboard(Color, Pawn, Rook, Knight, Bishop, Queen ^ mask, King);
            }

            if (pieceType.Is(Piece.King))
            {
                return new Bitboard(Color, Pawn, Rook, Knight, Bishop, Queen, King ^ mask);
            }

            throw new ArgumentOutOfRangeException(nameof(pieceType));
        }
    }
}
