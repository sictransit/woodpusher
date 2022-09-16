using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model
{
    internal struct Bitboard
    {
        // 00 .. 63
        // A1 .. H8

        public Bitboard(PieceColour colour, ulong pawn = 0, ulong rook = 0, ulong knight = 0, ulong bishop = 0, ulong queen = 0, ulong king = 0)
        {
            Colour = colour;
            Pawn = pawn;
            Rook = rook;
            Knight = knight;
            Bishop = bishop;
            Queen = queen;
            King = king;

            All = Pawn | Rook | Knight | Bishop | Queen | King;
        }

        public PieceColour Colour { get; }

        public ulong All { get; private set; }

        public ulong Pawn { get; init; }
        public ulong Rook { get; init; }
        public ulong Knight { get; init; }
        public ulong Bishop { get; init; }
        public ulong Queen { get; init; }
        public ulong King { get; init; }

        public bool IsOccupied(Square square) => IsOccupied(square.ToMask());

        public bool IsOccupied(ulong mask) => (All & mask) != 0;

        private Bitboard Add(PieceType pieceType, ulong mask)
        {
            if ((All & mask) != 0)
            {
                throw new InvalidOperationException("That square is already occupied.");
            }

            return Toggle(pieceType, mask);
        }

        public Bitboard Add(PieceType pieceType, Square square) => Add(pieceType, square.ToMask());

        private Bitboard Remove(PieceType pieceType, ulong mask)
        {
            if ((All & mask) == 0)
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
            for (int shift = 0; shift < 64; shift++)
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
            for (int shift = file; shift < 64; shift += 8)
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

            for (int shift = 0; shift < 64; shift++)
            {
                var mask = 1ul << shift;

                if ((bitmap & mask) != 0)
                {
                    yield return new Position(new Piece(type, Colour), mask.ToSquare());
                }
            }
        }

        public IEnumerable<Position> GetPieces(PieceType type, ulong mask)
        {
            var bitmap = GetBitmap(type);

            var colour = Colour;

            return (bitmap & mask).ToSquares().Select(s => new Position(new Piece(type, colour), s));
        }

        internal IEnumerable<Position> GetPieces(ulong mask)
        {
            for (int shift = 0; shift < 64; shift++)
            {
                var checkMask = mask & (1ul << shift);

                if (checkMask != 0 && IsOccupied(checkMask))
                {
                    yield return new Position(Peek(checkMask), checkMask.ToSquare());
                }
            }
        }

        public IEnumerable<Position> GetPieces(PieceType type, int file)
        {
            var bitmap = GetBitmap(type);

            for (int shift = file; shift < 64; shift += 8)
            {
                var mask = 1ul << shift;

                if ((bitmap & mask) != 0)
                {
                    yield return new Position(new Piece(type, Colour), mask.ToSquare());
                }
            }
        }

        private Piece Peek(ulong mask)
        {
            PieceType pieceType;

            if ((Pawn & mask) != 0)
            {
                pieceType = PieceType.Pawn;
            }
            else if ((Rook & mask) != 0)
            {
                pieceType = PieceType.Rook;
            }
            else if ((Knight & mask) != 0)
            {
                pieceType = PieceType.Knight;
            }
            else if ((Bishop & mask) != 0)
            {
                pieceType = PieceType.Bishop;
            }
            else if ((Queen & mask) != 0)
            {
                pieceType = PieceType.Queen;
            }
            else if ((King & mask) != 0)
            {
                pieceType = PieceType.King;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(mask));
            }

            return new Piece(pieceType, Colour);
        }

        private Bitboard Toggle(PieceType pieceType, ulong mask) => pieceType switch
        {
            PieceType.Pawn => new Bitboard(Colour, Pawn ^ mask, Rook, Knight, Bishop, Queen, King),
            PieceType.Rook => new Bitboard(Colour, Pawn, Rook ^ mask, Knight, Bishop, Queen, King),
            PieceType.Knight => new Bitboard(Colour, Pawn, Rook, Knight ^ mask, Bishop, Queen, King),
            PieceType.Bishop => new Bitboard(Colour, Pawn, Rook, Knight, Bishop ^ mask, Queen, King),
            PieceType.Queen => new Bitboard(Colour, Pawn, Rook, Knight, Bishop, Queen ^ mask, King),
            PieceType.King => new Bitboard(Colour, Pawn, Rook, Knight, Bishop, Queen, King ^ mask),
            _ => throw new ArgumentOutOfRangeException(nameof(pieceType)),
        };
    }
}
