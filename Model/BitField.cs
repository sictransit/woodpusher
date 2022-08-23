using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public struct BitField
    {
        // 00 .. 63
        // A1 .. H8

        public BitField(ulong pawn, ulong rook, ulong knight, ulong bishop, ulong queen, ulong king)
        {
            Pawn = pawn;
            Rook = rook;
            Knight = knight;
            Bishop = bishop;
            Queen = queen;
            King = king;
        }

        public ulong Aggregate => Pawn | Rook | Knight | Bishop | Queen | King;

        public ulong Pawn { get; init; }
        public ulong Rook { get; init; }
        public ulong Knight { get; init; }
        public ulong Bishop { get; init; }
        public ulong Queen { get; init; }
        public ulong King { get; init; }

        public bool IsOccupied(Square square) => IsOccupied(GetMask(square));

        public bool IsOccupied(ulong mask) => (Aggregate & mask) != 0;

        public BitField Add(PieceType pieceType, ulong mask)
        {
            if ((Aggregate & mask) != 0)
            {
                throw new InvalidOperationException("That square is already occupied.");
            }

            return Toggle(pieceType, mask);
        }

        public BitField Add(PieceType pieceType, Square square) => Add(pieceType, GetMask(square));

        public BitField Remove(PieceType pieceType, ulong mask)
        {
            if ((Aggregate & mask) == 0)
            {
                throw new InvalidOperationException("There is no piece on that square.");
            }

            return Toggle(pieceType, mask);
        }

        public BitField Remove(PieceType pieceType, Square square) => Remove(pieceType, GetMask(square));

        public PieceType? Peek(ulong mask) 
        {
            if ((Aggregate & mask) == 0)
            {
                return null;
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

        private BitField Toggle(PieceType pieceType, ulong mask)
        {
            var pawn = Pawn;
            var rook = Rook;
            var knight = Knight;
            var bishop = Bishop;
            var queen = Queen;
            var king = King;

            switch (pieceType)
            {
                case PieceType.Pawn:
                    pawn ^= mask;
                    break;
                case PieceType.Knight:
                    knight ^= mask;
                    break;
                case PieceType.Bishop:
                    bishop ^= mask;
                    break;
                case PieceType.Rook:
                    rook ^= mask;
                    break;
                case PieceType.Queen:
                    queen ^= mask;
                    break;
                case PieceType.King:
                    king ^= mask;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pieceType));
            }

            return new BitField(pawn, rook, knight, bishop, queen, king);
        }

        public static ulong GetMask(Square square) => 1ul << ((square.Rank << 3) + square.File);


    }
}
