using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Numerics;

namespace SicTransit.Woodpusher.Model
{
    public class Bitboard
    {
        // 00 .. 63
        // A1 .. H8

        public Bitboard(PieceColor color, ulong pawn = 0, ulong rook = 0, ulong knight = 0, ulong bishop = 0, ulong queen = 0, ulong king = 0)
        {
            Color = color;
            Pawn = pawn;
            Rook = rook;
            Knight = knight;
            Bishop = bishop;
            Queen = queen;
            King = king;

            All = Pawn | Rook | Knight | Bishop | Queen | King;
        }

        private PieceColor Color { get; }

        public ulong All { get; }

        public ulong Pawn { get; }
        public ulong Rook { get; }
        public ulong Knight { get; }
        public ulong Bishop { get; }
        public ulong Queen { get; }
        public ulong King { get; }

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
            for (var shift = 0; shift < 64; shift++)
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
                    yield return new Position(new Piece(type, Color), mask.ToSquare());
                }
            }
        }

        public IEnumerable<Position> GetPieces(PieceType type, ulong mask)
        {
            var bitmap = GetBitmap(type);

            var colour = this.Color;

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
                    yield return new Position(new Piece(type, Color), mask.ToSquare());
                }
            }
        }

        public Piece Peek(ulong mask)
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

            return new Piece(pieceType, Color);
        }

        private Bitboard Toggle(PieceType pieceType, ulong mask) => pieceType switch
        {
            PieceType.Pawn => new Bitboard(Color, Pawn ^ mask, Rook, Knight, Bishop, Queen, King),
            PieceType.Rook => new Bitboard(Color, Pawn, Rook ^ mask, Knight, Bishop, Queen, King),
            PieceType.Knight => new Bitboard(Color, Pawn, Rook, Knight ^ mask, Bishop, Queen, King),
            PieceType.Bishop => new Bitboard(Color, Pawn, Rook, Knight, Bishop ^ mask, Queen, King),
            PieceType.Queen => new Bitboard(Color, Pawn, Rook, Knight, Bishop, Queen ^ mask, King),
            PieceType.King => new Bitboard(Color, Pawn, Rook, Knight, Bishop, Queen, King ^ mask),
            _ => throw new ArgumentOutOfRangeException(nameof(pieceType)),
        };
    }
}
