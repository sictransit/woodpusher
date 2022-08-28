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

        public ulong Aggregate => white.All | black.All;

        public Board AddPiece(Square square, Piece piece)
        {
            return piece.HasFlag(Piece.White) ? new Board(white.Add(piece, square), black) : new Board(white, black.Add(piece, square));
        }

        public Board RemovePiece(Square square, Piece piece)
        {
            return piece.HasFlag(Piece.White) ? new Board(white.Remove(piece, square), black) : new Board(white, black.Remove(piece, square));
        }

        public bool IsOccupied(Square square)
        { 
            return white.IsOccupied(square) || black.IsOccupied(square);
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
            return white.IsOccupied(square) ? white.Peek(square) : black.Peek(square);
        }
    }
}
