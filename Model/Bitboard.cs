using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Model.Lookup;
using System.ComponentModel;
using System.Numerics;

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

        public bool IsOccupied(ulong mask) => (All & mask) != 0;

        public int Evaluation
        {
            get
            {
                var evaluation = 
                    BitOperations.PopCount(Pawn) * Scoring.PawnValue +
                    BitOperations.PopCount(Knight) * Scoring.KnightValue +
                    BitOperations.PopCount(Bishop) * Scoring.BishopValue +
                    BitOperations.PopCount(Rook) * Scoring.RookValue +
                    BitOperations.PopCount(Queen) * Scoring.QueenValue;

                // + 1 pawn for each pawn holding the center
                evaluation += BitOperations.PopCount(Pawn & Masks.CenterMask) * Scoring.PawnValue;
                
                return evaluation;
            }
        }

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

                var pieceType = Peek(mask);

                if (pieceType != PieceType.None)
                {
                    yield return new Position(new Piece(pieceType, color), mask);
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
                    yield return new Position(new Piece(type, color), mask);
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
                    yield return new Position(new Piece(type, color), mask);
                }
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
