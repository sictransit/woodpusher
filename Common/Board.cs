using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common;

public class Board : IBoard
{
    private readonly Bitboard white;
    private readonly Bitboard black;
    private readonly BoardInternals internals;

    public Counters Counters { get; }

    public Piece ActiveColor => Counters.ActiveColor;

    public Board() : this(new Bitboard(Piece.White), new Bitboard(Piece.None), Counters.Default)
    {

    }

    private Board(Bitboard white, Bitboard black, Counters counters, BoardInternals internals, ulong hash)
    {
        this.white = white;
        this.black = black;

        Counters = counters;
        this.internals = internals;
        Hash = hash == BoardInternals.InvalidHash ? internals.Zobrist.GetHash(this) : hash;
    }

    public Board(Bitboard white, Bitboard black, Counters counters) : this(white, black, counters, new BoardInternals(), BoardInternals.InvalidHash)
    {
    }

    public static IBoard StartingPosition()
    {
        var white = new Bitboard(Piece.White, 65280, 129, 66, 36, 8, 16);
        var black = new Bitboard(Piece.None, 71776119061217280, 9295429630892703744, 4755801206503243776, 2594073385365405696, 576460752303423488, 1152921504606846976);

        return new Board(white, black, Counters.Default);
    }

    public ulong Hash { get; }

    public int Score
    {
        get
        {
            var phase = white.Phase + black.Phase;

            var score = 0;

            foreach (var piece in white.GetPieces())
            {
                score += internals.Scoring.EvaluatePiece(piece, phase);
            }

            foreach (var piece in black.GetPieces())
            {
                score -= internals.Scoring.EvaluatePiece(piece, phase);
            }

            return score;
        }
    }    

    public IBoard SetPiece(Piece piece) => piece.Is(Piece.White)
            ? new Board(white.Toggle(piece), black, Counters, internals, BoardInternals.InvalidHash)
            : (IBoard)new Board(white, black.Toggle(piece), Counters, internals, BoardInternals.InvalidHash);

    public IBoard Play(Move move)
    {
        var hash = Hash;

        var whitePlaying = ActiveColor.Is(Piece.White);

        var opponentBitboard = whitePlaying ? black : white;

        var capture = opponentBitboard.Peek(move.EnPassantTarget == 0 ? move.Target : move.EnPassantMask);

        if (capture != Piece.None)
        {
            opponentBitboard = opponentBitboard.Toggle(capture);

            hash ^= internals.Zobrist.GetPieceHash(capture);
        }

        var activeBitboard = GetBitboard(ActiveColor).Toggle(move.Piece, move.Target);

        hash ^= internals.Zobrist.GetPieceHash(move.Piece) ^ internals.Zobrist.GetPieceHash(move.Piece.SetMask(move.Target));

        var castlings = Counters.Castlings;

        if (castlings != Castlings.None)
        {
            if (move.Piece.Is(Piece.King))
            {
                (ulong from, ulong to) castling = default;

                if (whitePlaying)
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

                    activeBitboard = activeBitboard.Toggle(rook, castling.to);

                    hash ^= internals.Zobrist.GetPieceHash(rook) ^ internals.Zobrist.GetPieceHash(rook.SetMask(castling.to));
                }
            }
            else
            {
                if (whitePlaying)
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

            hash ^= internals.Zobrist.GetCastlingsHash(Counters.Castlings) ^ internals.Zobrist.GetCastlingsHash(castlings);
        }

        if (move.Flags.HasFlag(SpecialMove.PawnPromotes))
        {
            var promotedPiece = move.Piece.SetMask(move.Target);
            var promotionPiece = (ActiveColor | move.PromotionType).SetMask(move.Target);
            activeBitboard = activeBitboard.Toggle(promotedPiece).Toggle(promotionPiece);

            hash ^= internals.Zobrist.GetPieceHash(promotedPiece) ^ internals.Zobrist.GetPieceHash(promotionPiece);
        }

        var counters = new Counters(
            whitePlaying ? Piece.None : Piece.White,
            castlings,
            move.EnPassantTarget,
            move.Piece.Is(Piece.Pawn) || capture.GetPieceType() != Piece.None ? 0 : Counters.HalfmoveClock + 1,
            Counters.FullmoveNumber + (whitePlaying ? 0 : 1),
            move,
            capture);

        hash ^= internals.Zobrist.GetMaskHash(Counters.EnPassantTarget)
            ^ internals.Zobrist.GetMaskHash(counters.EnPassantTarget)
            ^ internals.Zobrist.GetPieceHash(Counters.ActiveColor)
            ^ internals.Zobrist.GetPieceHash(counters.ActiveColor);

        return whitePlaying
            ? new Board(activeBitboard, opponentBitboard, counters, internals, hash)
            : new Board(opponentBitboard, activeBitboard, counters, internals, hash);
    }

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

        foreach (var knight in opponent.GetPieces(Piece.Knight, threats.Knight))
        {
            yield return knight;
        }

        foreach (var pawn in opponent.GetPieces(Piece.Pawn, threats.Pawn))
        {
            yield return pawn;
        }

        foreach (var king in opponent.GetPieces(Piece.King, threats.King))
        {
            yield return king;
        }

        var target = piece.GetMask();

        foreach (var bishop in opponent.GetPieces(Piece.Bishop, threats.Bishop))
        {
            if (!IsOccupied(internals.Moves.GetTravelMask(bishop.GetMask(), target)))
            {
                yield return bishop;
            }
        }

        foreach (var queen in opponent.GetPieces(Piece.Queen, threats.Queen))
        {
            if (!IsOccupied(internals.Moves.GetTravelMask(queen.GetMask(), target)))
            {
                yield return queen;
            }
        }

        foreach (var rook in opponent.GetPieces(Piece.Rook, threats.Rook))
        {
            if (!IsOccupied(internals.Moves.GetTravelMask(rook.GetMask(), target)))
            {
                yield return rook;
            }
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

    public IEnumerable<IBoard> PlayLegalMoves()
    {
        foreach (var piece in GetPieces(ActiveColor))
        {
            foreach (var board in PlayLegalMoves(piece))
            {
                yield return board;
            }
        }
    }

    public IEnumerable<IBoard> PlayLegalMoves(Piece piece)
    {
        var whiteIsPlaying = ActiveColor.Is(Piece.White);
        var friendlyBoard = whiteIsPlaying ? white : black;
        var hostileBoard = whiteIsPlaying ? black : white;

        foreach (var vector in internals.Moves.GetVectors(piece))
        {
            foreach (var move in vector)
            {
                if (friendlyBoard.IsOccupied(move.Target))
                {
                    break;
                }

                var hostileTarget = hostileBoard.Peek(move.Target);

                if (!ValidateMove(move, hostileTarget))
                {
                    break;
                }

                var board = Play(move);

                // Moving into check?
                if (board.IsAttacked(board.FindKing(ActiveColor)))
                {
                    if (hostileTarget != Piece.None)
                    {
                        break;
                    }

                    continue;
                }

                yield return board;

                if (hostileTarget != Piece.None)
                {
                    break;
                }
            }
        }
    }

    private bool ValidateMove(Move move, Piece hostileTarget)
    {
        if (move.Piece.Is(Piece.Pawn))
        {
            if (hostileTarget == Piece.None)
            {
                if (move.Flags.HasFlag(SpecialMove.PawnTakes))
                {
                    return false;
                }
            }
            else
            {
                if (move.Flags.HasFlag(SpecialMove.PawnMoves))
                {
                    return false;
                }
            }

            if (move.Flags.HasFlag(SpecialMove.PawnTakesEnPassant) && move.Target != Counters.EnPassantTarget)
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

    public bool IsChecked => IsAttacked(FindKing(ActiveColor));

    public bool IsAttacked(Piece piece) => GetPiecesInRange(piece, piece.OpponentColor()).Any();
}
