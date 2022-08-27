using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public struct Board
    {
        private Bitboard white;
        private Bitboard black;

        public Board() : this(new Bitboard(Piece.White), new Bitboard(Piece.Black))
        { 

        }

        internal Board(Bitboard white, Bitboard black)
        {
            this.white = white;
            this.black = black;
        }

        public ulong Aggregate => white.Aggregate | black.Aggregate;

        public Board AddPiece(Square square, Piece piece)
        {
            return piece.HasFlag(Piece.White) ? new Board(white.Add(piece, square), black) : new Board(white, black.Add(piece, square));
        }

        public Board RemovePiece(Square square, Piece piece)
        {
            return piece.HasFlag(Piece.White) ? new Board(white.Remove(piece, square), black) : new Board(white, black.Remove(piece, square));
        }

        public Piece Get(Square square)
        {
            var mask = Bitboard.GetMask(square);

            var piece = white.Peek(mask);

            if (piece != Piece.None)
            {
                return piece;
            }

            piece = black.Peek(mask);

            if (piece != Piece.None)
            {
                return piece;
            }

            return Piece.None;
        }
    }
}
