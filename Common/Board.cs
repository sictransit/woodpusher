using SicTransit.Woodpusher.Common.Lookup;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Numerics;

namespace SicTransit.Woodpusher.Common;

public class Board
{
    private readonly Bitboard white;
    private readonly Bitboard black;

    private readonly Bitboard opponentBoard;
    private readonly bool whiteIsPlaying;
    private readonly BoardInternals internals;

    private int? score = null;
    private int? phase = null;
    private bool? isChecked = null;
    private ulong? hash;

    public Counters Counters { get; }

    public Piece ActiveColor => Counters.ActiveColor;

    public Bitboard ActiveBoard { get; }

    public Board() : this(new Bitboard(Piece.White), new Bitboard(Piece.None), Counters.Default)
    {

    }

    private Board(Bitboard white, Bitboard black, Counters counters, BoardInternals internals, ulong? hash = null)
    {
        this.white = white;
        this.black = black;

        whiteIsPlaying = counters.ActiveColor.Is(Piece.White);
        ActiveBoard = whiteIsPlaying ? white : black;
        opponentBoard = whiteIsPlaying ? black : white;

        Counters = counters;

        this.internals = internals;

        this.hash = hash;
    }

    public Board(Bitboard white, Bitboard black, Counters counters) : this(white, black, counters, new BoardInternals())
    {
    }

    public static Board StartingPosition()
    {
        var white = new Bitboard(Piece.White,
            0xFF00,
            0x0081,
            0x0042,
            0x0024,
            0x0008,
            0x0010);
        var black = new Bitboard(Piece.None,
            0x00FF000000000000,
            0x8100000000000000,
            0x4200000000000000,
            0x2400000000000000,
            0x0800000000000000,
            0x1000000000000000);

        return new Board(white, black, Counters.Default);
    }

    public ulong Hash
    {
        get
        {
            hash ??= internals.Zobrist.GetHash(this);

            return hash.Value;
        }
    }

    public int Phase
    {
        get
        {
            phase ??= white.Phase + black.Phase;

            return phase.Value;
        }
    }

    public int Score
    {
        get
        {
            if (!score.HasValue)
            {
                score = 0;

                foreach (var (bitboard, sign) in new[] { (white, 1), (black, -1) })
                {
                    foreach (var piece in bitboard.GetPieces())
                    {
                        score += internals.Scoring.EvaluatePiece(piece, Phase) * sign;

                        // TODO: Not slow, but gives weird results. Needs to be fixed.
                        if (piece.Is(Piece.Pawn))
                        {
                            if (IsPassedPawn(piece))
                            {
                                score += Scoring.PassedPawnBonus * sign;
                            }

                            if (IsIsolatedPawn(piece))
                            {
                                score -= Scoring.IsolatedPawnPenalty * sign;
                            }
                        }
                    }

                    // Penalty for doubled pawns
                    for (var file = 0; file < 8; file++)
                    {
                        var pawnCount = BitOperations.PopCount(Bitboard.Files[file] & bitboard.Pawn);

                        if (pawnCount > 1)
                        {
                            var penalty = Scoring.DoubledPawnPenalty * ((2 << (pawnCount - 2)) - 1);
                            score -= penalty * sign;
                        }
                    }

                    // Penalty for single rook, bishop, knight
                    if (BitOperations.PopCount(bitboard.Rook) < 2)
                    {
                        score -= Scoring.SingleRookPenalty * sign;
                    }
                    if (BitOperations.PopCount(bitboard.Bishop) < 2)
                    {
                        score -= Scoring.SingleBishopPenalty * sign;
                    }
                    if (BitOperations.PopCount(bitboard.Knight) < 2)
                    {
                        score -= Scoring.SingleKnightPenalty * sign;
                    }
                }
            }

            return score.Value;
        }
    }

    public Board PlayNullMove()
    {
        // Toggle the active color
        var newActiveColor = ActiveColor == Piece.White ? Piece.None : Piece.White;

        // Update the counters
        var newCounters = new Counters(
            newActiveColor,
            Counters.Castlings,
            0, // No en passant target for null move
            Counters.HalfmoveClock + 1,
            Counters.FullmoveNumber + (ActiveColor == Piece.None ? 1 : 0),
            null,
            Piece.None // No capture for null move
        );

        // Calculate the new hash
        var newHash = Hash
            ^ internals.Zobrist.GetPieceHash(ActiveColor)
            ^ internals.Zobrist.GetPieceHash(newActiveColor)
            ^ internals.Zobrist.GetMaskHash(Counters.EnPassantTarget)
            ^ internals.Zobrist.GetMaskHash(newCounters.EnPassantTarget);

        // Return the new board state
        return new Board(white, black, newCounters, internals, newHash);
    }

    public Board SetPiece(Piece piece) => piece.Is(Piece.White)
            ? new Board(white.Toggle(piece), black, Counters, internals)
            : new Board(white, black.Toggle(piece), Counters, internals);

    public Board Play(Move move)
    {
        var newHash = Hash;

        var newActiveBoard = ActiveBoard.Toggle(move.Piece, move.Target);

        newHash ^= internals.Zobrist.GetPieceHash(move.Piece) ^ internals.Zobrist.GetPieceHash(move.Piece.SetMask(move.Target));

        var castlings = Counters.Castlings;

        if (castlings != Castlings.None)
        {
            if (move.Piece.Is(Piece.King))
            {
                (ulong from, ulong to) castling = default;

                if (whiteIsPlaying)
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
                    var rook = Piece.Rook | ActiveColor.SetMask(castling.from);

                    newActiveBoard = newActiveBoard.Toggle(rook, castling.to);

                    newHash ^= internals.Zobrist.GetPieceHash(rook) ^ internals.Zobrist.GetPieceHash(rook.SetMask(castling.to));
                }
            }
            else
            {
                if (whiteIsPlaying)
                {
                    if (move.Piece == BoardInternals.WhiteKingsideRook)
                    {
                        castlings &= ~Castlings.WhiteKingside;
                    }
                    else if (move.Piece == BoardInternals.WhiteQueensideRook)
                    {
                        castlings &= ~Castlings.WhiteQueenside;
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
                }

                if (move.Target == BoardInternals.BlackKingsideRookStartingSquare)
                {
                    castlings &= ~Castlings.BlackKingside;
                }
                else if (move.Target == BoardInternals.BlackQueensideRookStartingSquare)
                {
                    castlings &= ~Castlings.BlackQueenside;
                }
                else if (move.Target == BoardInternals.WhiteKingsideRookStartingSquare)
                {
                    castlings &= ~Castlings.WhiteKingside;
                }
                else if (move.Target == BoardInternals.WhiteQueensideRookStartingSquare)
                {
                    castlings &= ~Castlings.WhiteQueenside;
                }
            }

            newHash ^= internals.Zobrist.GetCastlingsHash(Counters.Castlings) ^ internals.Zobrist.GetCastlingsHash(castlings);
        }

        if (move.Flags.HasFlag(SpecialMove.PawnPromotes))
        {
            var promotedPiece = move.Piece.SetMask(move.Target);
            var promotionPiece = (ActiveColor | move.PromotionType).SetMask(move.Target);
            newActiveBoard = newActiveBoard.Toggle(promotedPiece).Toggle(promotionPiece);

            newHash ^= internals.Zobrist.GetPieceHash(promotedPiece) ^ internals.Zobrist.GetPieceHash(promotionPiece);
        }

        var capture = opponentBoard.Peek(move.EnPassantTarget == 0 ? move.Target : move.EnPassantMask);
        var newOpponentBoard = opponentBoard;

        if (capture != Piece.None)
        {
            newOpponentBoard = newOpponentBoard.Toggle(capture);

            newHash ^= internals.Zobrist.GetPieceHash(capture);
        }

        var counters = new Counters(
            whiteIsPlaying ? Piece.None : Piece.White,
            castlings,
            move.EnPassantTarget,
            move.Piece.Is(Piece.Pawn) || capture.GetPieceType() != Piece.None ? 0 : Counters.HalfmoveClock + 1,
            Counters.FullmoveNumber + (whiteIsPlaying ? 0 : 1),
            move,
            capture);

        newHash ^= internals.Zobrist.GetMaskHash(Counters.EnPassantTarget)
            ^ internals.Zobrist.GetMaskHash(counters.EnPassantTarget)
            ^ internals.Zobrist.GetPieceHash(Counters.ActiveColor)
            ^ internals.Zobrist.GetPieceHash(counters.ActiveColor);

        return whiteIsPlaying
            ? new Board(newActiveBoard, newOpponentBoard, counters, internals, newHash)
            : new Board(newOpponentBoard, newActiveBoard, counters, internals, newHash);
    }

    public bool IsPassedPawn(Piece piece) => (internals.Moves.GetPassedPawnMask(piece) & GetBitboard(piece.OpponentColor()).Pawn) == 0;

    public bool IsIsolatedPawn(Piece piece) => (internals.Moves.GetIsolatedPawnMask(piece) & GetBitboard(piece & Piece.White).Pawn) == 0;

    private bool IsOccupied(ulong mask) => ((white.AllPieces | black.AllPieces) & mask) != 0;

    private Bitboard GetBitboard(Piece color) => color.Is(Piece.White) ? white : black;

    public Piece FindKing(Piece color) => GetBitboard(color).GetKing();

    public IEnumerable<Piece> GetPieces() => GetPieces(Piece.White).Concat(GetPieces(Piece.None));

    public IEnumerable<Piece> GetPieces(Piece color) => GetBitboard(color).GetPieces();

    public IEnumerable<Piece> GetPieces(Piece color, Piece type) => GetPieces(color, type, ulong.MaxValue);

    private IEnumerable<Piece> GetPieces(Piece color, Piece type, ulong mask) => GetBitboard(color).GetPieces(type, mask);

    public IEnumerable<Piece> GetPiecesInRange(Piece piece, Piece color)
    {
        var threats = internals.Attacks.GetThreatMask(piece);

        var opponent = GetBitboard(color);

        if ((opponent.AllPieces & threats.All) == 0)
        {
            yield break;
        }

        var target = piece.GetMask();

        if ((opponent.Queen & threats.Queen) != 0)
        {
            foreach (var queen in opponent.GetPieces(Piece.Queen, threats.Queen))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(queen.GetMask(), target)))
                {
                    yield return queen;
                }
            }
        }

        if ((opponent.Bishop & threats.Bishop) != 0)
        {
            foreach (var bishop in opponent.GetPieces(Piece.Bishop, threats.Bishop))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(bishop.GetMask(), target)))
                {
                    yield return bishop;
                }
            }
        }

        if ((opponent.Rook & threats.Rook) != 0)
        {
            foreach (var rook in opponent.GetPieces(Piece.Rook, threats.Rook))
            {
                if (!IsOccupied(internals.Moves.GetTravelMask(rook.GetMask(), target)))
                {
                    yield return rook;
                }
            }
        }

        if ((opponent.Knight & threats.Knight) != 0)
        {
            foreach (var knight in opponent.GetPieces(Piece.Knight, threats.Knight))
            {
                yield return knight;
            }
        }

        if ((opponent.Pawn & threats.Pawn) != 0)
        {
            foreach (var pawn in opponent.GetPieces(Piece.Pawn, threats.Pawn))
            {
                yield return pawn;
            }
        }

        foreach (var king in opponent.GetPieces(Piece.King, threats.King))
        {
            yield return king;
        }
    }

    public IEnumerable<Move> GetLegalMoves()
    {
        return PlayLegalMoves().Select(b => b.Counters.LastMove);
    }

    public IEnumerable<Move> GetLegalMoves(Piece piece)
    {
        return PlayLegalMoves().Where(b => b.Counters.LastMove.Piece == piece).Select(b => b.Counters.LastMove);
    }

    public IEnumerable<Board> PlayLegalMoves(bool quiescence = false)
    {
        foreach (var piece in ActiveBoard.GetPieces())
        {
            foreach (var board in PlayLegalMoves(piece, quiescence))
            {
                yield return board;
            }
        }
    }

    private IEnumerable<Board> PlayLegalMoves(Piece piece, bool quiescence)
    {
        foreach (var vector in internals.Moves.GetVectors(piece))
        {
            foreach (var move in vector)
            {
                if (ActiveBoard.IsOccupied(move.Target))
                {
                    break;
                }

                var taking = opponentBoard.IsOccupied(move.Target);

                if (quiescence)
                {
                    if (move.Piece.Is(Piece.Pawn))
                    {
                        if (move.Flags.HasFlag(SpecialMove.PawnMoves))
                        {
                            continue;
                        }
                    }
                    else if (!taking)
                    {
                        continue;
                    }
                }

                if (!ValidateMove(move, taking))
                {
                    break;
                }

                var board = Play(move);

                // Moving into check?
                if (board.IsAttacked(board.FindKing(ActiveColor)))
                {
                    if (taking)
                    {
                        break;
                    }

                    continue;
                }

                yield return board;

                if (taking)
                {
                    break;
                }
            }
        }
    }

    private bool ValidateMove(Move move, bool taking)
    {
        if (move.Piece.Is(Piece.Pawn))
        {
            if (taking && move.Flags.HasFlag(SpecialMove.PawnMoves))
            {
                return false;
            }

            if (!taking && move.Flags.HasFlag(SpecialMove.PawnTakes))
            {
                return false;
            }

            if (move.Flags.HasFlag(SpecialMove.PawnTakesEnPassant) && move.Target != Counters.EnPassantTarget)
            {
                return false;
            }
        }
        else if (move.Piece.Is(Piece.King) && (move.Flags.HasFlag(SpecialMove.CastleQueen) || move.Flags.HasFlag(SpecialMove.CastleKing)))
        {
            if (move.Flags.HasFlag(SpecialMove.CastleQueen) && !Counters.Castlings.HasFlag(whiteIsPlaying ? Castlings.WhiteQueenside : Castlings.BlackQueenside))
            {
                return false;
            }

            if (move.Flags.HasFlag(SpecialMove.CastleKing) && !Counters.Castlings.HasFlag(whiteIsPlaying ? Castlings.WhiteKingside : Castlings.BlackKingside))
            {
                return false;
            }

            // castling path is blocked
            if (IsOccupied(move.CastlingEmptySquaresMask | move.CastlingCheckMask))
            {
                return false;
            }

            // castling from or into check
            if (IsAttacked(move.Piece) || IsAttacked(move.Piece.SetMask(move.CastlingCheckMask)) || IsAttacked(move.Piece.SetMask(move.Target)))
            {
                return false;
            }
        }

        return true;
    }

    public bool IsChecked
    {
        get
        {
            isChecked ??= IsAttacked(FindKing(ActiveColor));

            return isChecked.Value;
        }
    }

    public bool IsAttacked(Piece piece) => GetPiecesInRange(piece, piece.OpponentColor()).Any();
}
