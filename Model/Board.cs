using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Model.Lookup;

namespace SicTransit.Woodpusher.Model
{
    public class Board
    {
        private readonly Bitboard white;
        private readonly Bitboard black;

        public Counters Counters { get; init; }

        public Attacks Attacks { get; init; }

        public Moves Moves { get; init; }

        public PieceColour ActiveColour => Counters.ActiveColour;

        public Board() : this(new Bitboard(PieceColour.White), new Bitboard(PieceColour.Black), Counters.Default)
        {

        }

        public Board(Board board, Counters counters) : this(board.white, board.black, counters)
        {

        }

        internal static Board NewBoardCopyWhite(Board board, Bitboard black)
        {
            return new Board(board.white, black, board.Counters, board.Moves, board.Attacks);
        }

        internal static Board NewBoardCopyBlack(Board board, Bitboard white)
        {
            return new Board(white, board.black, board.Counters, board.Moves, board.Attacks);
        }


        internal Board(Bitboard white, Bitboard black, Counters counters, Moves? moves = null, Attacks? attacks = null)
        {
            this.white = white;
            this.black = black;

            Counters = counters;

            Moves = moves ?? new Moves();
            Attacks = attacks ?? new Attacks();
        }

        public Board AddPiece(Square square, Piece piece) => piece.Colour switch
        {
            PieceColour.White => NewBoardCopyBlack(this, white.Add(piece.Type, square)),
            PieceColour.Black => NewBoardCopyWhite(this, black.Add(piece.Type, square)),
            _ => throw new ArgumentOutOfRangeException(nameof(piece)),
        };

        public Board RemovePiece(Square square, Piece piece) => piece.Colour switch
        {
            PieceColour.White => NewBoardCopyBlack(this, white.Remove(piece.Type, square)),
            PieceColour.Black => NewBoardCopyWhite(this, black.Remove(piece.Type, square)),
            _ => throw new ArgumentOutOfRangeException(nameof(piece)),
        };

        public Board Play(Move move)
        {
            var whiteCastlings = Counters.WhiteCastlings;
            var blackCastlings = Counters.BlackCastlings;
            var fullmoveCounter = Counters.FullmoveNumber + (ActiveColour == PieceColour.Black ? 1 : 0);

            var opponentBitboard = GetBitboard(ActiveColour.OpponentColour());

            Piece? targetPiece = null;

            if (opponentBitboard.IsOccupied(move.Target.Square))
            {
                targetPiece = opponentBitboard.Peek(move.Target.Square);

                opponentBitboard = opponentBitboard.Remove(targetPiece.Value.Type, move.Target.Square);
            }

            var activeBitboard = GetBitboard(ActiveColour).Move(move.Position.Piece.Type, move.Position.Square, move.Target.Square);

            if (move.Position.Piece.Type == PieceType.King)
            {
                switch (ActiveColour)
                {
                    case PieceColour.White:
                        whiteCastlings = Castlings.None;
                        break;
                    case PieceColour.Black:
                        blackCastlings = Castlings.None;
                        break;
                }
            }

            var halfmoveClock = (move.Position.Piece.Type == PieceType.Pawn || targetPiece.HasValue) ? 0 : Counters.HalfmoveClock + 1;
            Square? enPassantTarget = null;

            if (move.Position.Piece.Type == PieceType.Pawn)
            {
                enPassantTarget = move.Target.ReferenceSquare;
            }

            var activeColour = ActiveColour.OpponentColour();

            var counters = new Counters(activeColour, whiteCastlings, blackCastlings, enPassantTarget, halfmoveClock, fullmoveCounter);

            return ActiveColour == PieceColour.White
                ? new Board(activeBitboard, opponentBitboard, counters, Moves, Attacks)
                : new Board(opponentBitboard, activeBitboard, counters, Moves, Attacks);
        }

        public bool IsOccupied(Square square) => white.IsOccupied(square) || black.IsOccupied(square);

        public bool IsOccupied(Square square, PieceColour colour) => GetBitboard(colour).IsOccupied(square);

        private Bitboard GetBitboard(PieceColour colour) => colour == PieceColour.White ? white : black;

        public Square FindKing(PieceColour colour) => GetBitboard(colour).King.ToSquare();

        public IEnumerable<Position> GetPositions(PieceColour colour) => GetBitboard(colour).GetPieces();

        public IEnumerable<Position> GetPositions(PieceColour colour, ulong mask) => GetBitboard(colour).GetPieces(mask);

        public IEnumerable<Position> GetPositions(PieceColour colour, int file) => GetBitboard(colour).GetPieces(file);

        public IEnumerable<Position> GetPositions(PieceColour colour, PieceType type) => GetBitboard(colour).GetPieces(type);

        public IEnumerable<Position> GetPositions(PieceColour colour, PieceType type, int file) => GetBitboard(colour).GetPieces(type, file);

        public Piece Get(Square square) => white.IsOccupied(square) ? white.Peek(square) : black.Peek(square);

        public IEnumerable<Position> GetAttackers(Square square)
        {
            if (!IsOccupied(square))
            {
                return Enumerable.Empty<Position>();
            }

            var targetPiece = Get(square);
            return null;

        }

    }
}
