using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public struct Board
    {
        // 00 .. 63
        // A1 .. H8

        public ulong Occupancy => White | Black;

        public ulong White => WhitePawn | WhiteRook | WhiteKnight | WhiteBishop | WhiteQueen | WhiteKing;
        public ulong Black => BlackPawn | BlackRook | BlackKnight | BlackBishop | BlackQueen | BlackKing;

        public ulong Pawns => WhitePawn | BlackPawn;
        public ulong Rooks => WhiteRook | BlackRook;
        public ulong Knights => WhiteKnight | BlackKnight;
        public ulong Bishops => WhiteBishop | BlackBishop;
        public ulong Queens => WhiteQueen | BlackQueen;
        public ulong Kings => WhiteKing | BlackKing;


        public ulong WhitePawn;
        public ulong WhiteRook;
        public ulong WhiteKnight;
        public ulong WhiteBishop;
        public ulong WhiteQueen;
        public ulong WhiteKing;

        public ulong BlackPawn;
        public ulong BlackRook;
        public ulong BlackKnight;
        public ulong BlackBishop;
        public ulong BlackQueen;
        public ulong BlackKing;

        public void Unset(Position position, Piece piece)
        {
            var mask = ~GetMask(position);

            if (piece.Colour == PieceColour.White)
            {
                switch (piece.Type)
                {
                    case PieceType.Pawn:
                        WhitePawn &= mask;
                        break;
                    case PieceType.Knight:
                        WhiteKnight &= mask;
                        break;
                    case PieceType.Bishop:
                        WhiteBishop &= mask;
                        break;
                    case PieceType.Rook:
                        WhiteRook &= mask;
                        break;
                    case PieceType.Queen:
                        WhiteQueen &= mask;
                        break;
                    case PieceType.King:
                        WhiteKing &= mask;
                        break;
                }
            }
            else
            {
                switch (piece.Type)
                {
                    case PieceType.Pawn:
                        BlackPawn &= mask;
                        break;
                    case PieceType.Knight:
                        BlackKnight &= mask;
                        break;
                    case PieceType.Bishop:
                        BlackBishop &= mask;
                        break;
                    case PieceType.Rook:
                        BlackRook &= mask;
                        break;
                    case PieceType.Queen:
                        BlackQueen &= mask;
                        break;
                    case PieceType.King:
                        BlackKing &= mask;
                        break;
                }
            }
        }

        public void Set(Position position, Piece piece)
        {
            var mask = GetMask(position);

            if (piece.Colour == PieceColour.White)
            {
                switch (piece.Type)
                {
                    case PieceType.Pawn:
                        WhitePawn |= mask;
                        break;
                    case PieceType.Knight:
                        WhiteKnight |= mask;
                        break;
                    case PieceType.Bishop:
                        WhiteBishop |= mask;
                        break;
                    case PieceType.Rook:
                        WhiteRook |= mask;
                        break;
                    case PieceType.Queen:
                        WhiteQueen |= mask;
                        break;
                    case PieceType.King:
                        WhiteKing |= mask;
                        break;
                }
            }
            else
            {
                switch (piece.Type)
                {
                    case PieceType.Pawn:
                        BlackPawn |= mask;
                        break;
                    case PieceType.Knight:
                        BlackKnight |= mask;
                        break;
                    case PieceType.Bishop:
                        BlackBishop |= mask;
                        break;
                    case PieceType.Rook:
                        BlackRook |= mask;
                        break;
                    case PieceType.Queen:
                        BlackQueen |= mask;
                        break;
                    case PieceType.King:
                        BlackKing |= mask;
                        break;
                }
            }
        }

        public Piece Get(Position position)
        {
            var mask = GetMask(position);

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
            if ((White & mask) != 0)
            {
                return PieceColour.White;
            }

            if ((Black & mask) != 0)
            {
                return PieceColour.Black;
            }

            return PieceColour.None;
        }

        private static ulong GetMask(Position position)
        {

            ulong p = 1u << position.File;

            int s = 8 * position.Rank;

            return p << s;
        }
    }
}
