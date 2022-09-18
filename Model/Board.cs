using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Model.Lookup;
using System.Numerics;

namespace SicTransit.Woodpusher.Model
{
    public class Board
    {
        private readonly Bitboard white;
        private readonly Bitboard black;

        public Counters Counters { get; }

        private Attacks Attacks { get; }

        private Moves Moves { get; }

        public PieceColor ActiveColor => Counters.ActiveColor;

        public Board() : this(new Bitboard(PieceColor.White), new Bitboard(PieceColor.Black), Counters.Default)
        {

        }

        public Board(Board board, Counters counters) : this(board.white, board.black, counters)
        {

        }

        public static Board Copy(Board board)
        {
            return new Board(board.white, board.black, board.Counters, board.Moves, board.Attacks);
        }

        private Board NewBoardCopyWhite(Bitboard blackBitboard)
        {
            return new Board(white, blackBitboard, Counters, Moves, Attacks);
        }

        private Board NewBoardCopyBlack(Bitboard whiteBitboard)
        {
            return new Board(whiteBitboard, black, Counters, Moves, Attacks);
        }

        private Board(Bitboard white, Bitboard black, Counters counters, Moves? moves = null, Attacks? attacks = null)
        {
            this.white = white;
            this.black = black;

            Counters = counters;

            Moves = moves ?? new Moves();
            Attacks = attacks ?? new Attacks();
        }

        public int Score
        {
            get
            {
                const int PAWN_VALUE = 1;
                const int KNIGHT_VALUE = 3;
                const int BISHOP_VALUE = 3;
                const int ROOK_VALUE = 5;
                const int QUEEN_VALUE = 9;
                const int KING_VALUE = 4711;

                var whiteEvaluation =
                    BitOperations.PopCount(white.Pawn) * PAWN_VALUE +
                    BitOperations.PopCount(white.Knight) * KNIGHT_VALUE +
                    BitOperations.PopCount(white.Bishop) * BISHOP_VALUE +
                    BitOperations.PopCount(white.Rook) * ROOK_VALUE +
                    BitOperations.PopCount(white.Queen) * QUEEN_VALUE +
                    BitOperations.PopCount(white.King) * KING_VALUE;

                var blackEvaluation =
                    BitOperations.PopCount(black.Pawn) * PAWN_VALUE +
                    BitOperations.PopCount(black.Knight) * KNIGHT_VALUE +
                    BitOperations.PopCount(black.Bishop) * BISHOP_VALUE +
                    BitOperations.PopCount(black.Rook) * ROOK_VALUE +
                    BitOperations.PopCount(black.Queen) * QUEEN_VALUE +
                    BitOperations.PopCount(black.King) * KING_VALUE;

                return whiteEvaluation - blackEvaluation;
            }
        }

        public Board AddPiece(Square square, Piece piece) => piece.Color switch
        {
            PieceColor.White => NewBoardCopyBlack(white.Add(piece.Type, square)),
            PieceColor.Black => NewBoardCopyWhite(black.Add(piece.Type, square)),
            _ => throw new ArgumentOutOfRangeException(nameof(piece)),
        };

        public Board RemovePiece(Square square, Piece piece) => piece.Color switch
        {
            PieceColor.White => NewBoardCopyBlack(white.Remove(piece.Type, square)),
            PieceColor.Black => NewBoardCopyWhite(black.Remove(piece.Type, square)),
            _ => throw new ArgumentOutOfRangeException(nameof(piece)),
        };

        public Board Play(Move move)
        {
            var whiteCastlings = Counters.WhiteCastlings;
            var blackCastlings = Counters.BlackCastlings;
            var fullmoveCounter = Counters.FullmoveNumber + (ActiveColor == PieceColor.Black ? 1 : 0);

            var opponentBitboard = GetBitboard(ActiveColor.OpponentColour());

            Piece? targetPiece = null;

            if (opponentBitboard.IsOccupied(move.Target.Square))
            {
                targetPiece = opponentBitboard.Peek(move.Target.Square);

                opponentBitboard = opponentBitboard.Remove(targetPiece.Value.Type, move.Target.Square);
            }

            var activeBitboard = GetBitboard(ActiveColor).Move(move.Position.Piece.Type, move.Position.Square, move.Target.Square);

            if (move.Position.Piece.Type == PieceType.King)
            {
                switch (ActiveColor)
                {
                    case PieceColor.White:
                        whiteCastlings = Castlings.None;
                        break;
                    case PieceColor.Black:
                        blackCastlings = Castlings.None;
                        break;
                }

                if (move.Target.Flags.HasFlag(SpecialMove.CastleQueen))
                {
                    activeBitboard = ActiveColor == PieceColor.White ?
                        activeBitboard.Move(PieceType.Rook, new Square("a1"), new Square("d1")) :
                        activeBitboard.Move(PieceType.Rook, new Square("a8"), new Square("d8"));
                }
                else if (move.Target.Flags.HasFlag(SpecialMove.CastleKing))
                {
                    activeBitboard = ActiveColor == PieceColor.White ?
                        activeBitboard.Move(PieceType.Rook, new Square("h1"), new Square("f1")) :
                        activeBitboard.Move(PieceType.Rook, new Square("h8"), new Square("f8"));
                }
            }

            var halfmoveClock = move.Position.Piece.Type == PieceType.Pawn || targetPiece.HasValue ? 0 : Counters.HalfmoveClock + 1;
            Square? enPassantTarget = null;

            if (move.Position.Piece.Type == PieceType.Pawn)
            {
                enPassantTarget = move.Target.EnPassantTarget;

                if (move.Target.Flags.HasFlag(SpecialMove.Promote))
                {
                    activeBitboard = activeBitboard.Remove(PieceType.Pawn, move.Target.Square).Add(move.Target.PromotionType, move.Target.Square);
                }
            }

            var activeColour = ActiveColor.OpponentColour();

            var counters = new Counters(activeColour, whiteCastlings, blackCastlings, enPassantTarget, halfmoveClock, fullmoveCounter);

            return ActiveColor == PieceColor.White
                ? new Board(activeBitboard, opponentBitboard, counters, Moves, Attacks)
                : new Board(opponentBitboard, activeBitboard, counters, Moves, Attacks);
        }

        public bool IsOccupied(Square square) => white.IsOccupied(square) || black.IsOccupied(square);

        private bool IsOccupied(ulong mask) => white.IsOccupied(mask) || black.IsOccupied(mask);

        private bool IsOccupied(Square square, PieceColor color) => GetBitboard(color).IsOccupied(square);

        private Bitboard GetBitboard(PieceColor color) => color == PieceColor.White ? white : black;

        public Square FindKing(PieceColor color) => GetBitboard(color).FindKing();

        public IEnumerable<Position> GetPositions(PieceColor color) => GetBitboard(color).GetPieces();

        public IEnumerable<Position> GetPositions(PieceColor color, int file) => GetBitboard(color).GetPieces(file);

        public IEnumerable<Position> GetPositions(PieceColor color, PieceType type) => GetBitboard(color).GetPieces(type);

        public IEnumerable<Position> GetPositions(PieceColor color, PieceType type, int file) => GetBitboard(color).GetPieces(type, file);

        private IEnumerable<Position> GetPositions(PieceColor color, PieceType type, ulong mask) => GetBitboard(color).GetPieces(type, mask);

        public Piece Get(Square square) => white.IsOccupied(square) ? white.Peek(square) : black.Peek(square);

        public IEnumerable<Position> GetAttackers(Square square)
        {
            if (!IsOccupied(square))
            {
                yield break;
            }

            var piece = Get(square);

            var threatMask = Attacks.GetThreatMask(piece.Color, square);

            var opponentColor = piece.Color.OpponentColour();

            foreach (var pawn in GetPositions(opponentColor, PieceType.Pawn, threatMask.PawnMask))
            {
                if (!IsOccupied(Moves.GetTravelMask(pawn.Square, square)))
                {
                    yield return pawn;
                }
            }

            foreach (var queen in GetPositions(opponentColor, PieceType.Queen, threatMask.QueenMask))
            {
                if (!IsOccupied(Moves.GetTravelMask(queen.Square, square)))
                {
                    yield return queen;
                }
            }

            foreach (var rook in GetPositions(opponentColor, PieceType.Rook, threatMask.RookMask))
            {
                if (!IsOccupied(Moves.GetTravelMask(rook.Square, square)))
                {
                    yield return rook;
                }
            }

            foreach (var knight in GetPositions(opponentColor, PieceType.Knight, threatMask.KnightMask))
            {
                if (!IsOccupied(Moves.GetTravelMask(knight.Square, square)))
                {
                    yield return knight;
                }
            }

            foreach (var bishop in GetPositions(opponentColor, PieceType.Bishop, threatMask.BishopMask))
            {
                if (!IsOccupied(Moves.GetTravelMask(bishop.Square, square)))
                {
                    yield return bishop;
                }
            }

            foreach (var king in GetPositions(opponentColor, PieceType.King, threatMask.KingMask))
            {
                if (!IsOccupied(Moves.GetTravelMask(king.Square, square)))
                {
                    yield return king;
                }
            }
        }

        public IEnumerable<Move> GetValidMoves()
        {
            foreach (var position in GetPositions(ActiveColor))
            {
                foreach (var move in GetValidMovesFromPosition(position))
                {
                    yield return move;
                }
            }
        }

        public IEnumerable<Move> GetValidMovesFromPosition(Position position)
        {
            foreach (IEnumerable<Target> vector in Moves.GetVectors(position))
            {
                foreach (var target in vector)
                {
                    var move = new Move(position, target);

                    if (!ValidateMove(move))
                    {
                        break;
                    }

                    if (IsCheck(move))
                    {
                        break;
                    }

                    yield return move;

                    if (TookPiece(move))
                    {
                        break;
                    }
                }
            }
        }

        private bool ValidateMove(Move move)
        {
            if (TakingOwnPiece(move))
            {
                return false;
            }

            if (MustTakeButCannot(move))
            {
                return false;
            }

            if (move.Position.Piece.Type == PieceType.Pawn)
            {
                if (EnPassantWithoutTarget(move))
                {
                    return false;
                }

                if (PawnCannotTakeForward(move))
                {
                    return false;
                }
            }

            if (move.Position.Piece.Type == PieceType.King)
            {
                var flags = move.Target.Flags;
                var castlings = ActiveColor == PieceColor.White ? Counters.WhiteCastlings : Counters.BlackCastlings;

                if ((flags.HasFlag(SpecialMove.CastleQueen) || flags.HasFlag(SpecialMove.CastleKing)))
                {
                    if (flags.HasFlag(SpecialMove.CastleQueen) && !castlings.HasFlag(Castlings.Queenside))
                    {
                        return false;
                    }

                    if (flags.HasFlag(SpecialMove.CastleKing) && !castlings.HasFlag(Castlings.Kingside))
                    {
                        return false;
                    }

                    if (CastlingFromOrIntoCheck(move))
                    {
                        return false;
                    }

                    if (CastlingPathIsBlocked(move))
                    {
                        return false;
                    }

                    if (CastlingRookIsMissing(move))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool IsCheck(Move move)
        {
            var testBoard = Play(move);

            return testBoard.IsAttacked(testBoard.FindKing(testBoard.ActiveColor.OpponentColour()));
        }

        private bool IsAttacked(Square square) => GetAttackers(square).Any();

        private bool CastlingFromOrIntoCheck(Move move) => IsAttacked(move.Position.Square) || IsAttacked(move.Target.CastlingCheckSquare!.Value) || IsAttacked(move.Target.Square);

        private bool CastlingPathIsBlocked(Move move) => move.Target.CastlingEmptySquares.Any(s => IsOccupied(s)) || IsOccupied(move.Target.CastlingCheckSquare.Value);

        private bool CastlingRookIsMissing(Move move) => !GetPositions(move.Position.Piece.Color, PieceType.Rook, move.Target.CastlingRookSquare.Value.ToMask()).Any();

        private bool TakingOwnPiece(Move move) => IsOccupied(move.Target.Square, move.Position.Piece.Color);

        private bool MustTakeButCannot(Move move) => move.Target.Flags.HasFlag(SpecialMove.MustTake) && !IsOccupied(move.Target.Square, move.Position.Piece.Color.OpponentColour());

        private bool PawnCannotTakeForward(Move move) => move.Target.Flags.HasFlag(SpecialMove.CannotTake) && IsOccupied(move.Target.Square);

        private bool EnPassantWithoutTarget(Move move) => move.Target.Flags.HasFlag(SpecialMove.EnPassant) && !move.Target.Square.Equals(Counters.EnPassantTarget);

        private bool TookPiece(Move move) => IsOccupied(move.Target.Square, move.Position.Piece.Color.OpponentColour());
    }
}
