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
        }

        public Piece Color { get; }

        public ulong Pawn { get; }
        public ulong Rook { get; }
        public ulong Knight { get; }
        public ulong Bishop { get; }
        public ulong Queen { get; }
        public ulong King { get; }

        public ulong All => Pawn | Rook | Knight | Bishop | Queen | King;

        public int Phase => BitOperations.PopCount(Knight) + BitOperations.PopCount(Bishop) + 2 * BitOperations.PopCount(Rook) + 4 * BitOperations.PopCount(Queen);

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

        public bool IsOccupied(ulong mask) => (All & mask) != 0;

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

        public Bitboard Toggle(Piece piece, ulong to = 0) => piece.GetPieceType() switch
        {
            Piece.Pawn => new Bitboard(Color, Pawn ^ (piece.GetMask() | to), Rook, Knight, Bishop, Queen, King),
            Piece.Rook => new Bitboard(Color, Pawn, Rook ^ (piece.GetMask() | to), Knight, Bishop, Queen, King),
            Piece.Knight => new Bitboard(Color, Pawn, Rook, Knight ^ (piece.GetMask() | to), Bishop, Queen, King),
            Piece.Bishop => new Bitboard(Color, Pawn, Rook, Knight, Bishop ^ (piece.GetMask() | to), Queen, King),
            Piece.Queen => new Bitboard(Color, Pawn, Rook, Knight, Bishop, Queen ^ (piece.GetMask() | to), King),
            Piece.King => new Bitboard(Color, Pawn, Rook, Knight, Bishop, Queen, King ^ (piece.GetMask() | to)),
            _ => throw new ArgumentOutOfRangeException(nameof(piece)),
        };
    }
}
