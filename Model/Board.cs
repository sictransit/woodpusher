using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model
{
    public struct Board
    {
        private readonly Bitboard white;
        private readonly Bitboard black;

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

        public IEnumerable<Position> GetPositions(Piece colour)
        {
            if (colour == Piece.White)
            {
                return white.GetPieces();
            }
            else
            {
                return black.GetPieces();
            }
        }

        public Piece Get(Square square)
        {
            var piece = white.Peek(square);

            if (piece != Piece.None)
            {
                return piece;
            }

            piece = black.Peek(square);

            if (piece != Piece.None)
            {
                return piece;
            }

            return Piece.None;
        }
    }
}
