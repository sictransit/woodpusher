using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Model.Interfaces;
using SicTransit.Woodpusher.Model.Lookup;

namespace SicTransit.Woodpusher.Model
{
    public class Board : IBoard
    {
        private readonly Bitboard white;
        private readonly Bitboard black;

        public Counters Counters { get; }

        private Attacks Attacks { get; }

        private Moves Moves { get; }

        private Masks Masks { get; }

        public PieceColor ActiveColor => Counters.ActiveColor;

        public Board() : this(new Bitboard(PieceColor.White), new Bitboard(PieceColor.Black), Counters.Default)
        {

        }

        public Board(Bitboard white, Bitboard black, Counters counters, Moves? moves = null, Attacks? attacks = null, Masks? masks = null)
        {
            this.white = white;
            this.black = black;

            Counters = counters;

            Moves = moves ?? new Moves();
            Attacks = attacks ?? new Attacks();
            Masks = masks ?? new Masks();
        }

        public int Score => white.Evaluation - black.Evaluation;

        public IBoard SetPosition(Position position) => position.Piece.Color switch
        {
            PieceColor.White => new Board(white.Add(position.Piece.Type, position.Current), black, Counters, Moves, Attacks, Masks),
            PieceColor.Black => new Board(white, black.Add(position.Piece.Type, position.Current), Counters, Moves, Attacks, Masks),
            _ => throw new ArgumentOutOfRangeException(nameof(position)),
        };

        public IBoard PlayMove(Move move)
        {
            return Play(move);
        }

        private Board Play(Move move)
        {
            var whiteCastlings = Counters.WhiteCastlings;
            var blackCastlings = Counters.BlackCastlings;
            var fullmoveCounter = Counters.FullmoveNumber + (ActiveColor == PieceColor.Black ? 1 : 0);

            var opponentBitboard = GetBitboard(ActiveColor.OpponentColour());

            PieceType targetPieceType = opponentBitboard.Peek(move.Target);

            if (targetPieceType != PieceType.None)
            {
                opponentBitboard = opponentBitboard.Remove(targetPieceType, move.Target);
            }
            else if (move.Flags.HasFlag(SpecialMove.EnPassant))
            {
                opponentBitboard = opponentBitboard.Remove(PieceType.Pawn, ActiveColor == PieceColor.White ? move.EnPassantTarget.AddFileAndRank(0, -1) : move.EnPassantTarget.AddFileAndRank(0, 1));
            }

            var activeBitboard = GetBitboard(ActiveColor).Move(move.Position.Piece.Type, move.Position.Current, move.Target);

            if (ActiveColor == PieceColor.White)
            {
                if (move.Position.Equals(Masks.WhiteKingsideRook))
                {
                    whiteCastlings &= ~Castlings.Kingside;
                }
                else if (move.Position.Equals(Masks.WhiteQueensideRook))
                {
                    whiteCastlings &= ~Castlings.Queenside;
                }

                if (move.Target == Masks.BlackKingsideRookStartingSquare)
                {
                    blackCastlings &= ~Castlings.Kingside;
                }
                else if (move.Target == Masks.BlackQueensideRookStartingSquare)
                {
                    blackCastlings &= ~Castlings.Queenside;
                }
            }
            else
            {
                if (move.Position.Equals(Masks.BlackKingsideRook))
                {
                    blackCastlings &= ~Castlings.Kingside;
                }
                else if (move.Position.Equals(Masks.BlackQueensideRook))
                {
                    blackCastlings &= ~Castlings.Queenside;
                }

                if (move.Target == Masks.WhiteKingsideRookStartingSquare)
                {
                    whiteCastlings &= ~Castlings.Kingside;
                }
                else if (move.Target == Masks.WhiteQueensideRookStartingSquare)
                {
                    whiteCastlings &= ~Castlings.Queenside;
                }

            }

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

                if (move.Flags.HasFlag(SpecialMove.CastleQueen))
                {
                    activeBitboard = ActiveColor == PieceColor.White ?
                        activeBitboard.Move(PieceType.Rook, Masks.WhiteQueensideRookStartingSquare, Masks.WhiteQueensideRookCastlingSquare) :
                        activeBitboard.Move(PieceType.Rook, Masks.BlackQueensideRookStartingSquare, Masks.BlackQueensideRookCastlingSquare);
                }
                else if (move.Flags.HasFlag(SpecialMove.CastleKing))
                {
                    activeBitboard = ActiveColor == PieceColor.White ?
                        activeBitboard.Move(PieceType.Rook, Masks.WhiteKingsideRookStartingSquare, Masks.WhiteKingsideRookCastlingSquare) :
                        activeBitboard.Move(PieceType.Rook, Masks.BlackKingsideRookStartingSquare, Masks.BlackKingsideRookCastlingSquare);
                }
            }

            var halfmoveClock = move.Position.Piece.Type == PieceType.Pawn || targetPieceType != PieceType.None ? 0 : Counters.HalfmoveClock + 1;

            if (move.Flags.HasFlag(SpecialMove.Promote))
            {
                activeBitboard = activeBitboard.Remove(PieceType.Pawn, move.Target).Add(move.PromotionType, move.Target);
            }

            var counters = new Counters(ActiveColor.OpponentColour(), whiteCastlings, blackCastlings, move.EnPassantTarget, halfmoveClock, fullmoveCounter);

            return ActiveColor == PieceColor.White
                ? new Board(activeBitboard, opponentBitboard, counters, Moves, Attacks, Masks)
                : new Board(opponentBitboard, activeBitboard, counters, Moves, Attacks, Masks);
        }

        private bool IsOccupied(ulong mask) => white.IsOccupied(mask) || black.IsOccupied(mask);

        private bool IsOccupied(ulong mask, PieceColor color) => GetBitboard(color).IsOccupied(mask);

        private Bitboard GetBitboard(PieceColor color) => color == PieceColor.White ? white : black;

        private ulong FindKing(PieceColor color) => GetBitboard(color).King;

        public IEnumerable<Position> GetPositions() => white.GetPieces().Concat(black.GetPieces());

        public IEnumerable<Position> GetPositions(PieceColor pieceColor) => GetBitboard(pieceColor).GetPieces();

        public IEnumerable<Position> GetPositions(PieceColor pieceColor, PieceType pieceType) => GetBitboard(pieceColor).GetPieces(pieceType);

        private IEnumerable<Position> GetPositions(PieceColor pieceColor, PieceType pieceType, ulong mask) => GetBitboard(pieceColor).GetPieces(pieceType, mask);

        public IEnumerable<Position> GetAttackers(ulong target, PieceColor color)
        {
            var threatMask = Attacks.GetThreatMask(color, target);

            var opponentColor = color.OpponentColour();

            foreach (var queen in GetPositions(opponentColor, PieceType.Queen, threatMask.QueenMask))
            {
                if (!IsOccupied(Moves.GetTravelMask(queen.Current, target)))
                {
                    yield return queen;
                }
            }

            foreach (var rook in GetPositions(opponentColor, PieceType.Rook, threatMask.RookMask))
            {
                if (!IsOccupied(Moves.GetTravelMask(rook.Current, target)))
                {
                    yield return rook;
                }
            }

            foreach (var bishop in GetPositions(opponentColor, PieceType.Bishop, threatMask.BishopMask))
            {
                if (!IsOccupied(Moves.GetTravelMask(bishop.Current, target)))
                {
                    yield return bishop;
                }
            }

            foreach (var pawn in GetPositions(opponentColor, PieceType.Pawn, threatMask.PawnMask))
            {
                if (!IsOccupied(Moves.GetTravelMask(pawn.Current, target)))
                {
                    yield return pawn;
                }
            }

            foreach (var knight in GetPositions(opponentColor, PieceType.Knight, threatMask.KnightMask))
            {
                if (!IsOccupied(Moves.GetTravelMask(knight.Current, target)))
                {
                    yield return knight;
                }
            }

            foreach (var king in GetPositions(opponentColor, PieceType.King, threatMask.KingMask))
            {
                if (!IsOccupied(Moves.GetTravelMask(king.Current, target)))
                {
                    yield return king;
                }
            }
        }

        public IEnumerable<Move> GetLegalMoves()
        {
            foreach (var position in GetPositions(ActiveColor))
            {
                foreach (var move in GetLegalMoves(position))
                {
                    yield return move;
                }
            }
        }

        public IEnumerable<Move> GetLegalMoves(Position position)
        {
            foreach (IEnumerable<Move> vector in Moves.GetVectors(position))
            {
                foreach (var move in vector)
                {
                    if (!ValidateMove(move))
                    {
                        break;
                    }

                    var tookPiece = TookPiece(move);

                    if (IsCheck(move))
                    {
                        if (tookPiece)
                        {
                            break;
                        }

                        continue;
                    }

                    yield return move;

                    if (tookPiece)
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

            if (move.Position.Piece.Type == PieceType.Pawn)
            {
                if (PawnCannotTakeForward(move))
                {
                    return false;
                }

                if (MustTakeButCannot(move))
                {
                    return false;
                }

                if (EnPassantWithoutTarget(move))
                {
                    return false;
                }
            }

            if (move.Position.Piece.Type == PieceType.King)
            {
                var flags = move.Flags;
                var castlings = ActiveColor == PieceColor.White ? Counters.WhiteCastlings : Counters.BlackCastlings;

                if (flags.HasFlag(SpecialMove.CastleQueen) || flags.HasFlag(SpecialMove.CastleKing))
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
                }
            }

            return true;
        }

        private bool IsCheck(Move move)
        {
            var testBoard = Play(move);

            return testBoard.IsAttacked(testBoard.FindKing(testBoard.ActiveColor.OpponentColour()), testBoard.ActiveColor.OpponentColour());
        }

        public bool IsChecked => IsAttacked(FindKing(ActiveColor), ActiveColor);

        private bool IsAttacked(ulong square, PieceColor color) => GetAttackers(square, color).Any();

        private bool CastlingFromOrIntoCheck(Move move) => IsAttacked(move.Position.Current, move.Position.Piece.Color) || IsAttacked(move.CastlingCheckMask, move.Position.Piece.Color) || IsAttacked(move.Target, move.Position.Piece.Color);

        private bool CastlingPathIsBlocked(Move move) => IsOccupied(move.CastlingEmptySquaresMask) || IsOccupied(move.CastlingCheckMask);

        private bool TakingOwnPiece(Move move) => IsOccupied(move.Target, move.Position.Piece.Color);

        private bool MustTakeButCannot(Move move) => move.Flags.HasFlag(SpecialMove.MustTake) && !IsOccupied(move.Target, move.Position.Piece.Color.OpponentColour());

        private bool PawnCannotTakeForward(Move move) => move.Flags.HasFlag(SpecialMove.CannotTake) && IsOccupied(move.Target);

        private bool EnPassantWithoutTarget(Move move) => move.Flags.HasFlag(SpecialMove.EnPassant) && move.Target != Counters.EnPassantTarget;

        private bool TookPiece(Move move) => IsOccupied(move.Target, move.Position.Piece.Color.OpponentColour());
    }
}
