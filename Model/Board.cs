﻿using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public struct Board
    {
        public Board(Bitboard white, Bitboard black)
        {
            White = white;
            Black = black;
        }

        public ulong Aggregate => White.Aggregate | Black.Aggregate;

        public Bitboard White { get; init; }
        public Bitboard Black { get; init; }

        public Board AddPiece(Square square, Piece piece)
        {
            return (piece.Colour == PieceColour.White) ? new Board(White.Add(piece.Type, square), Black) : new Board(White, Black.Add(piece.Type, square));
        }

        public Board RemovePiece(Square square, Piece piece)
        {
            return (piece.Colour == PieceColour.White) ? new Board(White.Remove(piece.Type, square), Black) : new Board(White, Black.Remove(piece.Type, square));
        }

        public Piece? Get(Square square)
        {
            var mask = Bitboard.GetMask(square);

            var pieceType = White.Peek(mask);

            if (pieceType != null)
            {
                return new Piece(PieceColour.White, pieceType.Value);
            }

            pieceType = Black.Peek(mask);

            if (pieceType != null)
            {
                return new Piece(PieceColour.Black, pieceType.Value);
            }

            return null;
        }
    }
}
