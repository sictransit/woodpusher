using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Lookup;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common
{
    public class Board : IBoard
    {
        private readonly Bitboard white;
        private readonly Bitboard black;
        private readonly BoardInternals internals;
        private readonly ulong occupiedSquares;

        public Counters Counters { get; }

        public PieceColor ActiveColor => Counters.ActiveColor;

        public Board() : this(new Bitboard(PieceColor.White), new Bitboard(PieceColor.Black), Counters.Default)
        {

        }

        private Board(Bitboard white, Bitboard black, Counters counters, BoardInternals internals)
        {
            this.white = white;
            this.black = black;
            occupiedSquares = white.All | black.All;

            Counters = counters;
            this.internals = internals;
        }

        public Board(Bitboard white, Bitboard black, Counters counters) : this(white, black, counters, new BoardInternals())
        {
        }

        public int Hash => HashCode.Combine(Counters.Hash, white.Hash, black.Hash);

        public int Score
        {
            get
            {
                var whiteEvaluation = GetPositions(PieceColor.White).Sum(p => internals.Scoring.EvaluatePosition(p, white.Queen == 0ul));
                var blackEvaluation = GetPositions(PieceColor.Black).Sum(p => internals.Scoring.EvaluatePosition(p, black.Queen == 0ul));

                whiteEvaluation += GetPositions(PieceColor.White, PieceType.Pawn).Count(IsPassedPawn) * Scoring.PawnValue / 2;
                blackEvaluation += GetPositions(PieceColor.Black, PieceType.Pawn).Count(IsPassedPawn) * Scoring.PawnValue / 2;

                return whiteEvaluation - blackEvaluation;
            }
        }

        public bool IsPassedPawn(Position position) => (internals.Moves.GetPassedPawnMask(position) & GetBitboard(position.Piece.Color.OpponentColour()).Pawn) == 0;

        public IBoard SetPosition(Position position) => position.Piece.Color switch
        {
            PieceColor.White => new Board(white.Add(position.Piece.Type, position.Current), black, Counters, internals),
            PieceColor.Black => new Board(white, black.Add(position.Piece.Type, position.Current), Counters, internals),
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

            var targetPieceType = opponentBitboard.Peek(move.Target);

            if (targetPieceType != PieceType.None)
            {
                opponentBitboard = opponentBitboard.Remove(targetPieceType, move.Target);
            }
            else if (move.Flags.HasFlag(SpecialMove.EnPassant))
            {
                opponentBitboard = opponentBitboard.Remove(PieceType.Pawn, ActiveColor == PieceColor.White ? move.EnPassantTarget.AddFileAndRank(0, -1) : move.EnPassantTarget.AddFileAndRank(0, 1));
            }

            var activeBitboard = GetBitboard(ActiveColor).Move(move.Position.Piece.Type, move.Position.Current, move.Target);

            if (whiteCastlings != Castlings.None || blackCastlings != Castlings.None)
            {
                if (ActiveColor == PieceColor.White)
                {
                    if (move.Position.Equals(BoardInternals.WhiteKingsideRook))
                    {
                        whiteCastlings &= ~Castlings.Kingside;
                    }
                    else if (move.Position.Equals(BoardInternals.WhiteQueensideRook))
                    {
                        whiteCastlings &= ~Castlings.Queenside;
                    }

                    if (move.Target == BoardInternals.BlackKingsideRookStartingSquare)
                    {
                        blackCastlings &= ~Castlings.Kingside;
                    }
                    else if (move.Target == BoardInternals.BlackQueensideRookStartingSquare)
                    {
                        blackCastlings &= ~Castlings.Queenside;
                    }
                }
                else
                {
                    if (move.Position.Equals(BoardInternals.BlackKingsideRook))
                    {
                        blackCastlings &= ~Castlings.Kingside;
                    }
                    else if (move.Position.Equals(BoardInternals.BlackQueensideRook))
                    {
                        blackCastlings &= ~Castlings.Queenside;
                    }

                    if (move.Target == BoardInternals.WhiteKingsideRookStartingSquare)
                    {
                        whiteCastlings &= ~Castlings.Kingside;
                    }
                    else if (move.Target == BoardInternals.WhiteQueensideRookStartingSquare)
                    {
                        whiteCastlings &= ~Castlings.Queenside;
                    }
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
                        activeBitboard.Move(PieceType.Rook, BoardInternals.WhiteQueensideRookStartingSquare, BoardInternals.WhiteQueensideRookCastlingSquare) :
                        activeBitboard.Move(PieceType.Rook, BoardInternals.BlackQueensideRookStartingSquare, BoardInternals.BlackQueensideRookCastlingSquare);
                }
                else if (move.Flags.HasFlag(SpecialMove.CastleKing))
                {
                    activeBitboard = ActiveColor == PieceColor.White ?
                        activeBitboard.Move(PieceType.Rook, BoardInternals.WhiteKingsideRookStartingSquare, BoardInternals.WhiteKingsideRookCastlingSquare) :
                        activeBitboard.Move(PieceType.Rook, BoardInternals.BlackKingsideRookStartingSquare, BoardInternals.BlackKingsideRookCastlingSquare);
                }
            }

            var halfmoveClock = move.Position.Piece.Type == PieceType.Pawn || targetPieceType != PieceType.None ? 0 : Counters.HalfmoveClock + 1;

            if (move.Flags.HasFlag(SpecialMove.Promote))
            {
                activeBitboard = activeBitboard.Remove(PieceType.Pawn, move.Target).Add(move.PromotionType, move.Target);
            }

            var counters = new Counters(ActiveColor.OpponentColour(), whiteCastlings, blackCastlings, move.EnPassantTarget, halfmoveClock, fullmoveCounter);

            return ActiveColor == PieceColor.White
                ? new Board(activeBitboard, opponentBitboard, counters, internals)
                : new Board(opponentBitboard, activeBitboard, counters, internals);
        }

        private bool IsOccupied(ulong mask) => (occupiedSquares & mask) != 0;

        public bool IsOccupied(ulong mask, PieceColor color) => GetBitboard(color).IsOccupied(mask);

        private Bitboard GetBitboard(PieceColor color) => color == PieceColor.White ? white : black;

        private ulong FindKing(PieceColor color) => GetBitboard(color).King;

        public IEnumerable<Position> GetPositions() => white.GetPieces().Concat(black.GetPieces());

        public IEnumerable<Position> GetPositions(PieceColor pieceColor) => GetBitboard(pieceColor).GetPieces();

        public IEnumerable<Position> GetPositions(PieceColor pieceColor, PieceType pieceType) => GetBitboard(pieceColor).GetPieces(pieceType);

        private IEnumerable<Position> GetPositions(PieceColor pieceColor, PieceType pieceType, ulong mask) => GetBitboard(pieceColor).GetPieces(pieceType, mask);

        public IEnumerable<Position> GetAttackers(ulong target, PieceColor color)
        {
            var threatMask = internals.Attacks.GetThreatMask(color, target);

            var opponentColor = color.OpponentColour();

            foreach (var queen in GetPositions(opponentColor, PieceType.Queen, threatMask.QueenMask))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(queen.Current, target)))
                {
                    yield return queen;
                }
            }

            foreach (var rook in GetPositions(opponentColor, PieceType.Rook, threatMask.RookMask))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(rook.Current, target)))
                {
                    yield return rook;
                }
            }

            foreach (var bishop in GetPositions(opponentColor, PieceType.Bishop, threatMask.BishopMask))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(bishop.Current, target)))
                {
                    yield return bishop;
                }
            }

            foreach (var pawn in GetPositions(opponentColor, PieceType.Pawn, threatMask.PawnMask))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(pawn.Current, target)))
                {
                    yield return pawn;
                }
            }

            foreach (var knight in GetPositions(opponentColor, PieceType.Knight, threatMask.KnightMask))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(knight.Current, target)))
                {
                    yield return knight;
                }
            }

            foreach (var king in GetPositions(opponentColor, PieceType.King, threatMask.KingMask))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(king.Current, target)))
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
            foreach (IReadOnlyCollection<Move> vector in internals.Moves.GetVectors(position))
            {
                foreach (var move in vector)
                {
                    if (!ValidateMove(move))
                    {
                        break;
                    }

                    var tookPiece = IsOccupied(move.Target, move.Position.Piece.Color.OpponentColour());

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
            // taking own piece
            if (IsOccupied(move.Target, move.Position.Piece.Color))
            {
                return false;
            }

            if (move.Position.Piece.Type == PieceType.Pawn)
            {
                if (move.Flags.HasFlag(SpecialMove.CannotTake) && IsOccupied(move.Target))
                {
                    return false;
                }

                if (move.Flags.HasFlag(SpecialMove.MustTake) && !IsOccupied(move.Target, move.Position.Piece.Color.OpponentColour()))
                {
                    return false;
                }

                if (move.Flags.HasFlag(SpecialMove.EnPassant) && move.Target != Counters.EnPassantTarget)
                {
                    return false;
                }
            }
            else if (move.Position.Piece.Type == PieceType.King)
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

                    // castling from or into check
                    if (IsAttacked(move.Position.Current, move.Position.Piece.Color) || IsAttacked(move.CastlingCheckMask, move.Position.Piece.Color) || IsAttacked(move.Target, move.Position.Piece.Color))
                    {
                        return false;
                    }

                    // castling path is blocked
                    if (IsOccupied(move.CastlingEmptySquaresMask) || IsOccupied(move.CastlingCheckMask))
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
    }
}
