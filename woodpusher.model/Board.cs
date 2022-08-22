using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using woodpusher.model.enums;

namespace woodpusher.model
{
    internal struct Board
    {
        // 00 .. 63
        // A1 .. H8

        public ulong Occupancy => WhitePawn | WhiteRook | WhiteKnight | WhiteBishop | WhiteQueen | WhiteKing | BlackPawn | BlackRook | BlackKnight | BlackBishop | BlackQueen | BlackKing;

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

        private static ulong GetMask(Position position) => 1u << position.File << 8 * position.Rank;
    }
}
