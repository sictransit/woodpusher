using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Security.Cryptography;

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

        public string GetHash()
        {
            // TODO: This is a really bad idea. We should adapt to e.g. Zobist hashing, but this will have to do for now.
            return BitConverter.ToString(Hash).Replace("-", "");
        }

        private byte[] Hash
        {
            get
            {
                using var md5 = MD5.Create();

                var bytes = white.Hash.Concat(black.Hash).Concat(Counters.Hash).ToArray();

                return md5.ComputeHash(bytes);
            }
        }

        public int Score
        {
            get
            {
                var phase = white.Phase + black.Phase;

                var whiteEvaluation = GetPositions(PieceColor.White).Sum(p => internals.Scoring.EvaluatePosition(p, phase));
                var blackEvaluation = GetPositions(PieceColor.Black).Sum(p => internals.Scoring.EvaluatePosition(p, phase));

                //whiteEvaluation += GetPositions(PieceColor.White, PieceType.Pawn).Count(IsPassedPawn) * Scoring.PawnValue / 2;
                //blackEvaluation += GetPositions(PieceColor.Black, PieceType.Pawn).Count(IsPassedPawn) * Scoring.PawnValue / 2;

                return whiteEvaluation - blackEvaluation;
            }
        }

        public IEnumerable<Move> GetOpeningBookMoves()
        {
            var moves = internals.OpeningBook.GetMoves(GetHash());

            if (!moves.Any())
            {
                yield break;
            }

            var legalMoves = GetLegalMoves().ToArray();

            foreach (var algebraicMove in moves)
            {
                yield return legalMoves.Single(m => m.ToAlgebraicMoveNotation().Equals(algebraicMove.Notation));
            }
        }

        public bool IsPassedPawn(Position position) => (internals.Moves.GetPassedPawnMask(position) & GetBitboard(position.Piece.Color.OpponentColor()).Pawn) == 0;

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
            var opponentBitboard = GetBitboard(ActiveColor.OpponentColor());

            var targetPieceType = opponentBitboard.Peek(move.Target);

            if (targetPieceType != PieceType.None)
            {
                opponentBitboard = opponentBitboard.Remove(targetPieceType, move.Target);
            }
            else if (move.Flags.HasFlag(SpecialMove.EnPassant))
            {
                opponentBitboard = opponentBitboard.Remove(PieceType.Pawn, move.EnPassantMask);
            }

            var activeBitboard = GetBitboard(ActiveColor).Move(move.Position.Piece.Type, move.Position.Current, move.Target);

            var whiteCastlings = Counters.WhiteCastlings;
            var blackCastlings = Counters.BlackCastlings;

            if (whiteCastlings != Castlings.None || blackCastlings != Castlings.None)
            {
                if (move.Position.Piece.Type == PieceType.King)
                {
                    (ulong from, ulong to) castling = default;

                    if (ActiveColor == PieceColor.White)
                    {
                        whiteCastlings = Castlings.None;

                        if (move.Flags.HasFlag(SpecialMove.CastleQueen))
                        {
                            castling = (BoardInternals.WhiteQueensideRookStartingSquare, BoardInternals.WhiteQueensideRookCastlingSquare);
                        }
                        else if (move.Flags.HasFlag(SpecialMove.CastleKing))
                        {
                            castling = (BoardInternals.WhiteKingsideRookStartingSquare, BoardInternals.WhiteKingsideRookCastlingSquare);
                        }
                    }
                    else
                    {
                        blackCastlings = Castlings.None;

                        if (move.Flags.HasFlag(SpecialMove.CastleQueen))
                        {
                            castling = (BoardInternals.BlackQueensideRookStartingSquare, BoardInternals.BlackQueensideRookCastlingSquare);
                        }
                        else if (move.Flags.HasFlag(SpecialMove.CastleKing))
                        {
                            castling = (BoardInternals.BlackKingsideRookStartingSquare, BoardInternals.BlackKingsideRookCastlingSquare);
                        }
                    }

                    if (castling != default)
                    {
                        activeBitboard = activeBitboard.Move(PieceType.Rook, castling.from, castling.to);
                    }
                }
                else
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
            }

            var halfmoveClock = move.Position.Piece.Type == PieceType.Pawn || targetPieceType != PieceType.None ? 0 : Counters.HalfmoveClock + 1;

            if (move.Flags.HasFlag(SpecialMove.Promote))
            {
                activeBitboard = activeBitboard.Remove(PieceType.Pawn, move.Target).Add(move.PromotionType, move.Target);
            }

            var fullmoveCounter = Counters.FullmoveNumber + (ActiveColor == PieceColor.Black ? 1 : 0);
            var counters = new Counters(ActiveColor.OpponentColor(), whiteCastlings, blackCastlings, move.EnPassantTarget, halfmoveClock, fullmoveCounter);

            return ActiveColor == PieceColor.White
                ? new Board(activeBitboard, opponentBitboard, counters, internals)
                : new Board(opponentBitboard, activeBitboard, counters, internals);
        }

        private bool IsOccupied(ulong mask) => (occupiedSquares & mask) != 0;

        private bool IsOccupied(ulong mask, PieceColor color) => GetBitboard(color).IsOccupied(mask);

        private Bitboard GetBitboard(PieceColor color) => color == PieceColor.White ? white : black;

        private ulong FindKing(PieceColor color) => GetBitboard(color).King;

        public IEnumerable<Position> GetPositions(PieceColor pieceColor) => GetBitboard(pieceColor).GetPositions();

        public IEnumerable<Position> GetPositions(PieceColor pieceColor, PieceType pieceType) => GetBitboard(pieceColor).GetPositions(pieceType);

        private IEnumerable<Position> GetPositions(PieceColor pieceColor, PieceType pieceType, ulong mask) => GetBitboard(pieceColor).GetPositions(pieceType, mask);

        public IEnumerable<Position> GetAttackers(ulong target, PieceColor color)
        {
            var threatMask = internals.Attacks.GetThreatMask(color, target);

            var opponentColor = color.OpponentColor();

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

                    var tookPiece = IsOccupied(move.Target);

                    if (IsMovingIntoCheck(move))
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

                if (move.Flags.HasFlag(SpecialMove.MustTake) && !IsOccupied(move.Target))
                {
                    return false;
                }

                if (move.Flags.HasFlag(SpecialMove.EnPassant) && move.Target != Counters.EnPassantTarget)
                {
                    return false;
                }
            }
            else if (move.Position.Piece.Type == PieceType.King && (move.Flags.HasFlag(SpecialMove.CastleQueen) || move.Flags.HasFlag(SpecialMove.CastleKing)))
            {
                var castlings = ActiveColor == PieceColor.White ? Counters.WhiteCastlings : Counters.BlackCastlings;

                if (move.Flags.HasFlag(SpecialMove.CastleQueen) && !castlings.HasFlag(Castlings.Queenside))
                {
                    return false;
                }

                if (move.Flags.HasFlag(SpecialMove.CastleKing) && !castlings.HasFlag(Castlings.Kingside))
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

            return true;
        }

        private bool IsMovingIntoCheck(Move move)
        {
            var testBoard = Play(move);

            var opponentColor = testBoard.ActiveColor.OpponentColor();

            return testBoard.IsAttacked(testBoard.FindKing(opponentColor), opponentColor);
        }

        public bool IsChecked => IsAttacked(FindKing(ActiveColor), ActiveColor);

        private bool IsAttacked(ulong square, PieceColor color) => GetAttackers(square, color).Any();
    }
}
