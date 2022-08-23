using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public struct Board
    {
        // 00 .. 63
        // A1 .. H8

        public ulong Occupancy => White.Aggregate | Black.Aggregate;

        public ulong Pawns => White.Pawn | Black.Pawn;
        public ulong Rooks => White.Rook | Black.Rook;
        public ulong Knights => White.Knight | Black.Knight;
        public ulong Bishops => White.Bishop | Black.Bishop;
        public ulong Queens => White.Queen | Black.Queen;
        public ulong Kings => White.King | Black.King;

        public BitField White;
        public BitField Black;

        public void Unset(Square square, Piece piece)
        {
            var mask = ~GetMask(square);

            if (piece.Colour == PieceColour.White)
            {
                switch (piece.Type)
                {
                    case PieceType.Pawn:
                        White.Pawn &= mask;
                        break;
                    case PieceType.Knight:
                        White.Knight &= mask;
                        break;
                    case PieceType.Bishop:
                        White.Bishop &= mask;
                        break;
                    case PieceType.Rook:
                        White.Rook &= mask;
                        break;
                    case PieceType.Queen:
                        White.Queen &= mask;
                        break;
                    case PieceType.King:
                        White.King &= mask;
                        break;
                }
            }
            else
            {
                switch (piece.Type)
                {
                    case PieceType.Pawn:
                        Black.Pawn &= mask;
                        break;
                    case PieceType.Knight:
                        Black.Knight &= mask;
                        break;
                    case PieceType.Bishop:
                        Black.Bishop &= mask;
                        break;
                    case PieceType.Rook:
                        Black.Rook &= mask;
                        break;
                    case PieceType.Queen:
                        Black.Queen &= mask;
                        break;
                    case PieceType.King:
                        Black.King &= mask;
                        break;
                }
            }
        }

        public void Set(Square square, Piece piece)
        {
            var mask = GetMask(square);

            if (piece.Colour == PieceColour.White)
            {
                switch (piece.Type)
                {
                    case PieceType.Pawn:
                        White.Pawn |= mask;
                        break;
                    case PieceType.Knight:
                        White.Knight |= mask;
                        break;
                    case PieceType.Bishop:
                        White.Bishop |= mask;
                        break;
                    case PieceType.Rook:
                        White.Rook |= mask;
                        break;
                    case PieceType.Queen:
                        White.Queen |= mask;
                        break;
                    case PieceType.King:
                        White.King |= mask;
                        break;
                }
            }
            else
            {
                switch (piece.Type)
                {
                    case PieceType.Pawn:
                        Black.Pawn |= mask;
                        break;
                    case PieceType.Knight:
                        Black.Knight |= mask;
                        break;
                    case PieceType.Bishop:
                        Black.Bishop |= mask;
                        break;
                    case PieceType.Rook:
                        Black.Rook |= mask;
                        break;
                    case PieceType.Queen:
                        Black.Queen |= mask;
                        break;
                    case PieceType.King:
                        Black.King |= mask;
                        break;
                }
            }
        }

        public Piece Get(Square square)
        {
            var mask = GetMask(square);

            var colour = GetPieceColour(mask);

            return colour == PieceColour.None ? Piece.Empty : new Piece(colour, GetPieceType(mask));
        }

        private PieceType GetPieceType(ulong mask)
        {
            if ((Pawns & mask) != 0)
            {
                return PieceType.Pawn;
            }

            if ((Knights & mask) != 0)
            {
                return PieceType.Knight;
            }

            if ((Bishops & mask) != 0)
            {
                return PieceType.Bishop;
            }

            if ((Rooks & mask) != 0)
            {
                return PieceType.Rook;
            }

            if ((Queens & mask) != 0)
            {
                return PieceType.Queen;
            }

            if ((Kings & mask) != 0)
            {
                return PieceType.King;
            }

            return PieceType.None;
        }

        private PieceColour GetPieceColour(ulong mask)
        {
            if ((White.Aggregate & mask) != 0)
            {
                return PieceColour.White;
            }

            if ((Black.Aggregate & mask) != 0)
            {
                return PieceColour.Black;
            }

            return PieceColour.None;
        }

        private static ulong GetMask(Square square) => 1u << ((square.Rank << 3) + square.File);
    }
}
