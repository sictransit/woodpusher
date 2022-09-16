using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model
{
    internal class Bitboard
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

            all = this.pawn | this.rook | this.knight | this.bishop | this.queen | this.king;
        }

        private readonly PieceColor color;

        private readonly ulong all;

        private readonly ulong pawn;
        private readonly ulong rook;
        private readonly ulong knight;
        private readonly ulong bishop;
        private readonly ulong queen;
        private readonly ulong king;

        public Square FindKing() => king.ToSquare();

        public bool IsOccupied(Square square) => IsOccupied(square.ToMask());

        public bool IsOccupied(ulong mask) => (all & mask) != 0;

        private Bitboard Add(PieceType pieceType, ulong mask)
        {
            if ((all & mask) != 0)
            {
                throw new InvalidOperationException("That square is already occupied.");
            }

            return Toggle(pieceType, mask);
        }

        public Bitboard Add(PieceType pieceType, Square square) => Add(pieceType, square.ToMask());

        private Bitboard Remove(PieceType pieceType, ulong mask)
        {
            if ((all & mask) == 0)
            {
                throw new InvalidOperationException("There is no piece on that square.");
            }

            return Toggle(pieceType, mask);
        }

        public Bitboard Remove(PieceType pieceType, Square square) => Remove(pieceType, square.ToMask());

        public Bitboard Move(PieceType pieceType, Square fromSquare, Square toSquare)
        {
            return Toggle(pieceType, fromSquare.ToMask() | toSquare.ToMask());
        }

        public Piece Peek(Square square) => Peek(square.ToMask());

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

        public IEnumerable<Position> GetPieces()
        {
            for (var shift = 0; shift < 64; shift++)
            {
                var mask = 1ul << shift;

                if (IsOccupied(mask))
                {
                    yield return new Position(Peek(mask), mask.ToSquare());
                }
            }
        }

        public IEnumerable<Position> GetPieces(int file)
        {
            for (var shift = file; shift < 64; shift += 8)
            {
                var mask = 1ul << shift;

                if (IsOccupied(mask))
                {
                    yield return new Position(Peek(mask), mask.ToSquare());
                }
            }
        }

        public IEnumerable<Position> GetPieces(PieceType type)
        {
            var bitmap = GetBitmap(type);

            for (var shift = 0; shift < 64; shift++)
            {
                var mask = 1ul << shift;

                if ((bitmap & mask) != 0)
                {
                    yield return new Position(new Piece(type, color), mask.ToSquare());
                }
            }
        }

        public IEnumerable<Position> GetPieces(PieceType type, ulong mask)
        {
            var bitmap = GetBitmap(type);

            var colour = this.color;

            return (bitmap & mask).ToSquares().Select(s => new Position(new Piece(type, colour), s));
        }

        public IEnumerable<Position> GetPieces(PieceType type, int file)
        {
            var bitmap = GetBitmap(type);

            for (var shift = file; shift < 64; shift += 8)
            {
                var mask = 1ul << shift;

                if ((bitmap & mask) != 0)
                {
                    yield return new Position(new Piece(type, color), mask.ToSquare());
                }
            }
        }

        private Piece Peek(ulong mask)
        {
            PieceType pieceType;

            if ((pawn & mask) != 0)
            {
                pieceType = PieceType.Pawn;
            }
            else if ((rook & mask) != 0)
            {
                pieceType = PieceType.Rook;
            }
            else if ((knight & mask) != 0)
            {
                pieceType = PieceType.Knight;
            }
            else if ((bishop & mask) != 0)
            {
                pieceType = PieceType.Bishop;
            }
            else if ((queen & mask) != 0)
            {
                pieceType = PieceType.Queen;
            }
            else if ((king & mask) != 0)
            {
                pieceType = PieceType.King;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(mask));
            }

            return new Piece(pieceType, color);
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
