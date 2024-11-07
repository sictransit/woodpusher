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
            this.color = color;

            this.pawn = pawn;
            this.rook = rook;
            this.knight = knight;
            this.bishop = bishop;
            this.queen = queen;
            this.king = king;

            AllPieces = pawn | rook | knight | bishop | queen | king;
        }

        private readonly Piece color;

        private readonly ulong pawn;
        private readonly ulong rook;
        private readonly ulong knight;
        private readonly ulong bishop;
        private readonly ulong queen;
        private readonly ulong king;        
        
        public ulong AllPieces { get; }

        public int Phase => BitOperations.PopCount(knight) + BitOperations.PopCount(bishop) + 2 * BitOperations.PopCount(rook) + 4 * BitOperations.PopCount(queen);

        private ulong GetBitmap(Piece pieceType) => pieceType switch
        {
            Piece.Pawn => pawn,
            Piece.Knight => knight,
            Piece.Bishop => bishop,
            Piece.Rook => rook,
            Piece.Queen => queen,
            Piece.King => king,
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

                yield return (type | color).SetMask(bit);

                bitmap &= ~bit;
            }
        }

        public bool IsOccupied(ulong mask) => (AllPieces & mask) != 0;

        public Piece GetKing() => color | Piece.King.SetMask(king);

        public Piece Peek(ulong mask)
        {
            if ((AllPieces & mask) == 0)
            {
                return Piece.None;
            }

            if ((pawn & mask) != 0)
            {
                return color | Piece.Pawn.SetMask(mask);
            }

            if ((rook & mask) != 0)
            {
                return color | Piece.Rook.SetMask(mask);
            }

            if ((knight & mask) != 0)
            {
                return color | Piece.Knight.SetMask(mask);
            }

            if ((bishop & mask) != 0)
            {
                return color | Piece.Bishop.SetMask(mask);
            }

            if ((queen & mask) != 0)
            {
                return color | Piece.Queen.SetMask(mask);
            }

            return color | Piece.King.SetMask(mask);
        }

        public Bitboard Toggle(Piece piece, ulong to = 0) => piece.GetPieceType() switch
        {
            Piece.Pawn => new Bitboard(color, pawn ^ (piece.GetMask() | to), rook, knight, bishop, queen, king),
            Piece.Rook => new Bitboard(color, pawn, rook ^ (piece.GetMask() | to), knight, bishop, queen, king),
            Piece.Knight => new Bitboard(color, pawn, rook, knight ^ (piece.GetMask() | to), bishop, queen, king),
            Piece.Bishop => new Bitboard(color, pawn, rook, knight, bishop ^ (piece.GetMask() | to), queen, king),
            Piece.Queen => new Bitboard(color, pawn, rook, knight, bishop, queen ^ (piece.GetMask() | to), king),
            Piece.King => new Bitboard(color, pawn, rook, knight, bishop, queen, king ^ (piece.GetMask() | to)),
            _ => throw new ArgumentOutOfRangeException(nameof(piece)),
        };
    }
}
