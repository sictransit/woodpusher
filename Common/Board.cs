﻿using SicTransit.Woodpusher.Common.Interfaces;
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

        public Pieces ActiveColor => Counters.ActiveColor;

        public Board() : this(new Bitboard(Pieces.White), new Bitboard(Pieces.None), Counters.Default)
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

                var whiteEvaluation = GetPieces(Pieces.White).Sum(p => internals.Scoring.EvaluatePiece(p, phase));
                var blackEvaluation = GetPieces(Pieces.None).Sum(p => internals.Scoring.EvaluatePiece(p, phase));

                //whiteEvaluation += GetPieces(Pieces.White, Pieces.Pawn).Count(IsPassedPawn) * Scoring.PawnValue / 2;
                //blackEvaluation += GetPieces(Pieces.Black, Pieces.Pawn).Count(IsPassedPawn) * Scoring.PawnValue / 2;

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

        public bool IsPassedPawn(Pieces piece) => (internals.Moves.GetPassedPawnMask(piece) & GetBitboard(piece.OpponentColor()).Pawn) == 0;

        public IBoard SetPiece(Pieces piece) => piece.Is(Pieces.White)
                ? new Board(white.Add(piece), black, Counters, internals)
                : (IBoard)new Board(white, black.Add(piece), Counters, internals);

        public IBoard PlayMove(Move move)
        {
            return Play(move);
        }

        private Board Play(Move move)
        {
            var opponentBitboard = GetBitboard(ActiveColor.OpponentColor());

            var targetPieceType = opponentBitboard.Peek(move.Target);

            if (targetPieceType != Pieces.None)
            {
                opponentBitboard = opponentBitboard.Remove(targetPieceType);
            }
            else if (move.Flags.HasFlag(SpecialMove.EnPassant))
            {
                opponentBitboard = opponentBitboard.Remove(Pieces.Pawn.SetMask(move.EnPassantMask));
            }

            var activeBitboard = GetBitboard(ActiveColor).Move(move.Piece, move.Target);

            var whiteCastlings = Counters.WhiteCastlings;
            var blackCastlings = Counters.BlackCastlings;

            if (whiteCastlings != Castlings.None || blackCastlings != Castlings.None)
            {
                if (move.Piece.Is(Pieces.King))
                {
                    (ulong from, ulong to) castling = default;

                    if (ActiveColor.Is(Pieces.White))
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
                        activeBitboard = activeBitboard.Move(Pieces.Rook.SetMask(castling.from), castling.to);
                    }
                }
                else
                {
                    if (ActiveColor.Is(Pieces.White))
                    {
                        if (move.Piece.Equals(BoardInternals.WhiteKingsideRook))
                        {
                            whiteCastlings &= ~Castlings.Kingside;
                        }
                        else if (move.Piece.Equals(BoardInternals.WhiteQueensideRook))
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
                        if (move.Piece.Equals(BoardInternals.BlackKingsideRook))
                        {
                            blackCastlings &= ~Castlings.Kingside;
                        }
                        else if (move.Piece.Equals(BoardInternals.BlackQueensideRook))
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

            var halfmoveClock = (move.Piece.Is(Pieces.Pawn) || targetPieceType.GetPieceType() != Pieces.None) ? 0 : Counters.HalfmoveClock + 1;

            if (move.Flags.HasFlag(SpecialMove.Promote))
            {
                activeBitboard = activeBitboard.Remove(Pieces.Pawn.SetMask(move.Target)).Add(move.PromotionType.SetMask(move.Target));
            }

            var fullmoveCounter = Counters.FullmoveNumber + (ActiveColor.Is(Pieces.White) ? 0 : 1);
            var counters = new Counters(ActiveColor.OpponentColor(), whiteCastlings, blackCastlings, move.EnPassantTarget, halfmoveClock, fullmoveCounter);

            return ActiveColor.Is(Pieces.White)
                ? new Board(activeBitboard, opponentBitboard, counters, internals)
                : new Board(opponentBitboard, activeBitboard, counters, internals);
        }

        private bool IsOccupied(ulong mask) => (occupiedSquares & mask) != 0;

        private bool IsOccupied(ulong mask, Pieces color) => GetBitboard(color).IsOccupied(mask);

        private Bitboard GetBitboard(Pieces color) => color.Is(Pieces.White) ? white : black;

        private ulong FindKing(Pieces color) => GetBitboard(color).King;

        public IEnumerable<Pieces> GetPieces(Pieces pieceColor) => GetBitboard(pieceColor).GetPieces();

        public IEnumerable<Pieces> GetPieces(Pieces pieceColor, Pieces pieceType) => GetBitboard(pieceColor).GetPieces(pieceType);

        private IEnumerable<Pieces> GetPieces(Pieces pieceColor, Pieces pieceType, ulong mask) => GetBitboard(pieceColor).GetPieces(pieceType, mask);

        public IEnumerable<Pieces> GetAttackers(ulong target, Pieces color)
        {
            var threatMask = internals.Attacks.GetThreatMask(color.SetMask(target));

            var opponentColor = color.OpponentColor();

            foreach (var queen in GetPieces(opponentColor, Pieces.Queen, threatMask.QueenMask))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(queen.GetMask(), target)))
                {
                    yield return queen;
                }
            }

            foreach (var rook in GetPieces(opponentColor, Pieces.Rook, threatMask.RookMask))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(rook.GetMask(), target)))
                {
                    yield return rook;
                }
            }

            foreach (var bishop in GetPieces(opponentColor, Pieces.Bishop, threatMask.BishopMask))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(bishop.GetMask(), target)))
                {
                    yield return bishop;
                }
            }

            foreach (var pawn in GetPieces(opponentColor, Pieces.Pawn, threatMask.PawnMask))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(pawn.GetMask(), target)))
                {
                    yield return pawn;
                }
            }

            foreach (var knight in GetPieces(opponentColor, Pieces.Knight, threatMask.KnightMask))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(knight.GetMask(), target)))
                {
                    yield return knight;
                }
            }

            foreach (var king in GetPieces(opponentColor, Pieces.King, threatMask.KingMask))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(king.GetMask(), target)))
                {
                    yield return king;
                }
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

        public IEnumerable<Move> GetLegalMoves(Pieces piece)
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
            if (IsOccupied(move.Target, move.Piece.GetColor()))
            {
                return false;
            }

            if (move.Piece.Is(Pieces.Pawn))
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
            else if (move.Piece.Is(Pieces.King) && (move.Flags.HasFlag(SpecialMove.CastleQueen) || move.Flags.HasFlag(SpecialMove.CastleKing)))
            {
                var castlings = ActiveColor == Pieces.White ? Counters.WhiteCastlings : Counters.BlackCastlings;

                if (move.Flags.HasFlag(SpecialMove.CastleQueen) && !castlings.HasFlag(Castlings.Queenside))
                {
                    return false;
                }

                if (move.Flags.HasFlag(SpecialMove.CastleKing) && !castlings.HasFlag(Castlings.Kingside))
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

            var opponentColor = testBoard.ActiveColor.OpponentColor();

            return testBoard.IsAttacked(testBoard.FindKing(opponentColor), opponentColor);
        }

        public bool IsChecked => IsAttacked(FindKing(ActiveColor), ActiveColor);

        private bool IsAttacked(ulong square, Pieces color) => GetAttackers(square, color).Any();
    }
}
