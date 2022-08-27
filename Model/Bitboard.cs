using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    internal struct Bitboard
    {
        // 00 .. 63
        // A1 .. H8

        public Bitboard(Piece colour) : this(colour, 0, 0, 0, 0, 0, 0)
        { 
        }

        public Bitboard(Piece colour, ulong pawn, ulong rook, ulong knight, ulong bishop, ulong queen, ulong king)
        {
            Colour = colour;
            Pawn = pawn;
            Rook = rook;
            Knight = knight;
            Bishop = bishop;
            Queen = queen;
            King = king;
        }

        public ulong Aggregate => Pawn | Rook | Knight | Bishop | Queen | King;

        public Piece Colour { get; }
        public ulong Pawn { get; init; }
        public ulong Rook { get; init; }
        public ulong Knight { get; init; }
        public ulong Bishop { get; init; }
        public ulong Queen { get; init; }
        public ulong King { get; init; }

        public bool IsOccupied(Square square) => IsOccupied(GetMask(square));

        public bool IsOccupied(ulong mask) => (Aggregate & mask) != 0;

        public Bitboard Add(Piece piece, ulong mask)
        {
            if ((Aggregate & mask) != 0)
            {
                throw new InvalidOperationException("That square is already occupied.");
            }

            return Toggle(piece, mask);
        }

        public Bitboard Add(Piece piece, Square square) => Add(piece, GetMask(square));

        public Bitboard Remove(Piece piece, ulong mask)
        {
            if ((Aggregate & mask) == 0)
            {
                throw new InvalidOperationException("There is no piece on that square.");
            }

            return Toggle(piece, mask);
        }

        public Bitboard Remove(Piece piece, Square square) => Remove(piece, GetMask(square));

        public Piece Peek(ulong mask)
        {
            if ((Pawn & mask) != 0)
            {
                return Colour | Piece.Pawn;
            }

            if ((Rook & mask) != 0)
            {
                return Colour | Piece.Rook;
            }

            if ((Knight & mask) != 0)
            {
                return Colour | Piece.Knight;                
            }

            if ((Bishop & mask) != 0)
            {
                return Colour | Piece.Bishop;                
            }

            if ((Queen & mask) != 0)
            {
                return Colour | Piece.Queen;                
            }

            if ((King & mask) != 0)
            {
                return Colour | Piece.King;
            }

            return Piece.None;
        }

        private Bitboard Toggle(Piece piece, ulong mask)
        {
            if (piece.HasFlag(Piece.Pawn))
            {
                return new Bitboard(Colour, Pawn ^ mask, Rook, Knight, Bishop, Queen, King);
            }
            else if (piece.HasFlag(Piece.Rook))
            {
                return new Bitboard(Colour, Pawn, Rook ^mask, Knight, Bishop, Queen, King);
            }
            else if (piece.HasFlag(Piece.Knight))
            {
                return new Bitboard(Colour, Pawn, Rook, Knight ^ mask, Bishop, Queen, King);
            }
            else if (piece.HasFlag(Piece.Bishop))
            {
                return new Bitboard(Colour, Pawn, Rook, Knight, Bishop ^mask, Queen, King);
            }
            else if (piece.HasFlag(Piece.Queen))
            {
                return new Bitboard(Colour, Pawn, Rook , Knight, Bishop, Queen ^mask, King);
            }
            else if (piece.HasFlag(Piece.King))
            {
                return new Bitboard(Colour, Pawn, Rook, Knight, Bishop, Queen , King ^mask);
            }

            throw new ArgumentOutOfRangeException(nameof(piece));
        }

        public static ulong GetMask(Square square) => 1ul << ((square.Rank << 3) + square.File);
    }
}
