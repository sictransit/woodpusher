using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model
{
    public struct Board
    {
        private readonly Bitboard white;
        private readonly Bitboard black;

        public Counters Counters { get; init; }

        public Board() : this(new Bitboard(PieceColour.White), new Bitboard(PieceColour.Black), Counters.Default)
        {

        }

        public Board(Board board, Counters counters) : this(board.white, board.black, counters)
        {

        }

        internal Board(Bitboard white, Bitboard black, Counters counters)
        {
            this.white = white;
            this.black = black;
            Counters = counters;
        }

        public Board AddPiece(Square square, Piece piece) => piece.Colour switch
        {
            PieceColour.White => new Board(white.Add(piece.Type, square), black, Counters),
            PieceColour.Black => new Board(white, black.Add(piece.Type, square), Counters),
            _ => throw new ArgumentOutOfRangeException(nameof(piece)),
        };

        public Board RemovePiece(Square square, Piece piece) => piece.Colour switch
        {
            PieceColour.White => new Board(white.Remove(piece.Type, square), black, Counters),
            PieceColour.Black => new Board(white, black.Remove(piece.Type, square), Counters),
            _ => throw new ArgumentOutOfRangeException(nameof(piece)),
        };

        public bool IsOccupied(Square square) => white.IsOccupied(square) || black.IsOccupied(square);

        public bool IsOccupied(Square square, PieceColour colour) => GetBitboard(colour).IsOccupied(square);

        private Bitboard GetBitboard(PieceColour colour) => colour == PieceColour.White ? white : black;

        public Square FindKing(PieceColour colour) => GetBitboard(colour).King.ToSquare();

        public IEnumerable<Position> GetPositions(PieceColour colour) => GetBitboard(colour).GetPieces();

        public IEnumerable<Position> GetPositionsOnFile(PieceColour colour, int file) => GetBitboard(colour).GetPieces(file);

        public Piece Get(Square square) => white.IsOccupied(square) ? white.Peek(square) : black.Peek(square);
    }
}
