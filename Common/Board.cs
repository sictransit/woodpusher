using Serilog;
using SicTransit.Woodpusher.Common.Interfaces;
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

        public Piece ActiveColor => Counters.ActiveColor;

        public Board() : this(new Bitboard(Piece.White), new Bitboard(Piece.None), Counters.Default)
        {

        }

        private Board(Bitboard white, Bitboard black, Counters counters, BoardInternals internals, ulong hash)
        {
            this.white = white;
            this.black = black;
            occupiedSquares = white.All | black.All;

            Counters = counters;
            this.internals = internals;
            Hash = hash == BoardInternals.InvalidHash ? internals.Zobrist.GetHash(this) : hash;
        }

        public Board(Bitboard white, Bitboard black, Counters counters) : this(white, black, counters, new BoardInternals(), BoardInternals.InvalidHash)
        {
        }

        public ulong Hash { get; }

        public int Score
        {
            get
            {
                var phase = white.Phase + black.Phase;

                var score = 0;

                foreach (var piece in GetPieces())
                {
                    var evaluation = internals.Scoring.EvaluatePiece(piece, phase);

                    //var attackers = GetAttackers(piece.GetMask(), piece.GetColor()).Count();
                    //var defenders = GetAttackers(piece.GetMask(), piece.GetColor().OpponentColor()).Count();

                    //if (attackers > defenders)
                    //{
                    //    evaluation /= 2;
                    //}

                    switch (piece.GetPieceType())
                    {
                        case Piece.Pawn:
                            if (IsPassedPawn(piece))
                            {
                                evaluation *= 2;
                            }
                            break;
                        case Piece.Knight:
                            break;
                        case Piece.Bishop:
                            break;
                        case Piece.Rook:
                            break;
                        case Piece.Queen:
                            break;
                        case Piece.King:
                            break;
                        default:
                            break;
                    }

                    score += piece.Is(Piece.White) ? evaluation : -evaluation;
                }

                return score;
            }
        }

        public IEnumerable<Move> GetOpeningBookMoves()
        {
            var moves = internals.OpeningBook.GetMoves(Hash);

            if (!moves.Any())
            {
                yield break;
            }

            var legalMoves = GetLegalMoves().ToArray();

            foreach (var algebraicMove in moves)
            {
                var move = legalMoves.Single(m => m.ToAlgebraicMoveNotation().Equals(algebraicMove.Notation));

                Log.Information($"Found opening book move: {move}");

                yield return move;
            }
        }

        public bool IsPassedPawn(Piece piece) => (internals.Moves.GetPassedPawnMask(piece) & GetBitboard(piece.OpponentColor()).Pawn) == 0;

        public IBoard SetPiece(Piece piece) => piece.Is(Piece.White)
                ? new Board(white.Add(piece), black, Counters, internals, BoardInternals.InvalidHash)
                : (IBoard)new Board(white, black.Add(piece), Counters, internals, BoardInternals.InvalidHash);

        public IBoard PlayMove(Move move)
        {
            return Play(move);
        }

        private Board Play(Move move)
        {
            var hash = Hash;

            var opponentBitboard = GetBitboard(ActiveColor.OpponentColor());

            var capture = opponentBitboard.Peek(move.EnPassantTarget == 0 ? move.Target : move.EnPassantMask);

            if (capture != Piece.None)
            {
                opponentBitboard = opponentBitboard.Remove(capture);

                hash ^= internals.Zobrist.GetPieceHash(capture);
            }

            var activeBitboard = GetBitboard(ActiveColor).Move(move.Piece, move.Target);

            hash ^= internals.Zobrist.GetPieceHash(move.Piece) ^ internals.Zobrist.GetPieceHash(move.Piece.SetMask(move.Target));

            var castlings = Counters.Castlings;

            if (castlings != Castlings.None)
            {
                if (move.Piece.Is(Piece.King))
                {
                    (ulong from, ulong to) castling = default;

                    if (ActiveColor.Is(Piece.White))
                    {
                        castlings &= ~(Castlings.WhiteKingside | Castlings.WhiteQueenside);

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
                        castlings &= ~(Castlings.BlackKingside | Castlings.BlackQueenside);

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
                        var rook = (Piece.Rook | ActiveColor).SetMask(castling.from);

                        activeBitboard = activeBitboard.Move(rook, castling.to);

                        hash ^= internals.Zobrist.GetPieceHash(rook) ^ internals.Zobrist.GetPieceHash(rook.SetMask(castling.to));
                    }
                }
                else
                {
                    if (ActiveColor.Is(Piece.White))
                    {
                        if (move.Piece == BoardInternals.WhiteKingsideRook)
                        {
                            castlings &= ~Castlings.WhiteKingside;
                        }
                        else if (move.Piece == BoardInternals.WhiteQueensideRook)
                        {
                            castlings &= ~Castlings.WhiteQueenside;
                        }

                        if (move.Target == BoardInternals.BlackKingsideRookStartingSquare)
                        {
                            castlings &= ~Castlings.BlackKingside;
                        }
                        else if (move.Target == BoardInternals.BlackQueensideRookStartingSquare)
                        {
                            castlings &= ~Castlings.BlackQueenside;
                        }
                    }
                    else
                    {
                        if (move.Piece == BoardInternals.BlackKingsideRook)
                        {
                            castlings &= ~Castlings.BlackKingside;
                        }
                        else if (move.Piece == BoardInternals.BlackQueensideRook)
                        {
                            castlings &= ~Castlings.BlackQueenside;
                        }

                        if (move.Target == BoardInternals.WhiteKingsideRookStartingSquare)
                        {
                            castlings &= ~Castlings.WhiteKingside;
                        }
                        else if (move.Target == BoardInternals.WhiteQueensideRookStartingSquare)
                        {
                            castlings &= ~Castlings.WhiteQueenside;
                        }
                    }
                }

                hash ^= internals.Zobrist.GetCastlingsHash(Counters.Castlings) ^ internals.Zobrist.GetCastlingsHash(castlings);
            }

            if (move.Flags.HasFlag(SpecialMove.Promote))
            {
                var promotedPiece = move.Piece.SetMask(move.Target);
                var promotionPiece = (ActiveColor | move.PromotionType).SetMask(move.Target);
                activeBitboard = activeBitboard.Remove(promotedPiece).Add(promotionPiece);

                hash ^= internals.Zobrist.GetPieceHash(promotedPiece) ^ internals.Zobrist.GetPieceHash(promotionPiece);
            }

            var halfmoveClock = move.Piece.Is(Piece.Pawn) || capture.GetPieceType() != Piece.None ? 0 : Counters.HalfmoveClock + 1;

            var fullmoveCounter = Counters.FullmoveNumber + (ActiveColor.Is(Piece.White) ? 0 : 1);
            var counters = new Counters(ActiveColor.OpponentColor(), castlings, move.EnPassantTarget, halfmoveClock, fullmoveCounter, capture);

            hash ^= internals.Zobrist.GetMaskHash(Counters.EnPassantTarget) ^ internals.Zobrist.GetMaskHash(counters.EnPassantTarget);
            hash ^= internals.Zobrist.GetPieceHash(Counters.ActiveColor) ^ internals.Zobrist.GetPieceHash(counters.ActiveColor);

            return ActiveColor.Is(Piece.White)
                ? new Board(activeBitboard, opponentBitboard, counters, internals, hash)
                : new Board(opponentBitboard, activeBitboard, counters, internals, hash);
        }

        private bool IsOccupied(ulong mask) => (occupiedSquares & mask) != 0;

        private bool IsOccupied(ulong mask, Piece color) => GetBitboard(color).IsOccupied(mask);

        private Bitboard GetBitboard(Piece color) => color.Is(Piece.White) ? white : black;

        private ulong FindKing(Piece color) => GetBitboard(color).King;

        public IEnumerable<Piece> GetPieces() => GetPieces(Piece.White).Concat(GetPieces(Piece.None));

        public IEnumerable<Piece> GetPieces(Piece color) => GetBitboard(color).GetPieces();

        public IEnumerable<Piece> GetPieces(Piece color, Piece type) => GetPieces(color, type, ulong.MaxValue);

        private IEnumerable<Piece> GetPieces(Piece color, Piece type, ulong mask) => GetBitboard(color).GetPieces(type, mask);

        public IEnumerable<Piece> GetAttackers(ulong target, Piece color)
        {
            var threats = internals.Attacks.GetThreatMask(color.SetMask(target));

            var opponentColor = color.OpponentColor();

            if (!IsOccupied(threats.Any, opponentColor))
            {
                yield break;
            }

            foreach (var pawn in GetPieces(opponentColor, Piece.Pawn, threats.Pawn))
            {
                yield return pawn;
            }

            foreach (var knight in GetPieces(opponentColor, Piece.Knight, threats.Knight))
            {
                yield return knight;
            }

            foreach (var queen in GetPieces(opponentColor, Piece.Queen, threats.Queen))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(queen.GetMask(), target)))
                {
                    yield return queen;
                }
            }

            foreach (var rook in GetPieces(opponentColor, Piece.Rook, threats.Rook))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(rook.GetMask(), target)))
                {
                    yield return rook;
                }
            }

            foreach (var bishop in GetPieces(opponentColor, Piece.Bishop, threats.Bishop))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(bishop.GetMask(), target)))
                {
                    yield return bishop;
                }
            }

            foreach (var king in GetPieces(opponentColor, Piece.King, threats.King))
            {
                yield return king;
            }
        }

        public IEnumerable<Move> GetLegalMoves()
        {
            foreach (var piece in GetPieces(ActiveColor))
            {
                foreach (var move in GetLegalMoves(piece))
                {
                    yield return move;
                }
            }
        }

        public IEnumerable<Move> GetLegalMoves(Piece piece)
        {
            foreach (IReadOnlyCollection<Move> vector in internals.Moves.GetVectors(piece))
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
            if (IsOccupied(move.Target, move.Piece))
            {
                return false;
            }

            if (move.Piece.Is(Piece.Pawn))
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
            else if (move.Piece.Is(Piece.King) && (move.Flags.HasFlag(SpecialMove.CastleQueen) || move.Flags.HasFlag(SpecialMove.CastleKing)))
            {
                if (move.Flags.HasFlag(SpecialMove.CastleQueen) && !Counters.Castlings.HasFlag(ActiveColor.Is(Piece.White) ? Castlings.WhiteQueenside : Castlings.BlackQueenside))
                {
                    return false;
                }

                if (move.Flags.HasFlag(SpecialMove.CastleKing) && !Counters.Castlings.HasFlag(ActiveColor.Is(Piece.White) ? Castlings.WhiteKingside : Castlings.BlackKingside))
                {
                    return false;
                }

                // castling from or into check
                if (IsAttacked(move.Piece.GetMask(), move.Piece.GetColor()) || IsAttacked(move.CastlingCheckMask, move.Piece.GetColor()) || IsAttacked(move.Target, move.Piece.GetColor()))
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

            return testBoard.IsAttacked(testBoard.FindKing(ActiveColor), ActiveColor);
        }

        public bool IsChecked => IsAttacked(FindKing(ActiveColor), ActiveColor);

        private bool IsAttacked(ulong square, Piece color) => GetAttackers(square, color).Any();
    }
}
