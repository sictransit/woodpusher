using SicTransit.Woodpusher.Model.Enums;
using System.Numerics;

namespace SicTransit.Woodpusher.Model.Extensions;

public static class PieceExtensions
{
    private const int SquareMask = 0x00ff;
    private const int PieceMask = 0xff00;

    public static Piece SetMask(this Piece p, ulong mask) => (Piece)(((int)p & PieceMask) | (1 + BitOperations.TrailingZeroCount(mask)));

    public static ulong GetMask(this Piece p) => 1ul << (((int)p & SquareMask) - 1);

    public static Piece SetPiece(this Piece p, Piece piece) => (Piece)((int)p & SquareMask) | piece;

    public static Piece GetPiece(this Piece p) => (Piece)((int)p & PieceMask);

    public static Piece GetColor(this Piece p) => p.Is(Piece.White) ? Piece.White : Piece.None;

    public static Piece GetPieceType(this Piece p) => p.GetPiece() & ~Piece.White;

    public static Square GetSquare(this Piece p) => p.GetMask().ToSquare();

    public static Piece SetSquare(this Piece p, Square square) => p.SetMask(square.ToMask());

    public static bool Is(this Piece p, Piece pieceType) => p.HasFlag(pieceType);

    public static Piece OpponentColor(this Piece p) => p.Is(Piece.White) ? Piece.None : Piece.White;

    public static char ToChar(this Piece p) =>
        p.GetPieceType() switch
        {
            Piece.Pawn => 'P',
            Piece.Knight => 'N',
            Piece.Bishop => 'B',
            Piece.Rook => 'R',
            Piece.Queen => 'Q',
            Piece.King => 'K',
            _ => throw new ArgumentOutOfRangeException(nameof(p))
        };

    public static char ToAlgebraicNotation(this Piece p) => p.Is(Piece.White) ? p.ToChar() : char.ToLowerInvariant(p.ToChar());

    public static Piece ToPieceType(this char c) => c switch
    {
        'P' => Piece.Pawn,
        'R' => Piece.Rook,
        'N' => Piece.Knight,
        'B' => Piece.Bishop,
        'Q' => Piece.Queen,
        'K' => Piece.King,
        _ => throw new NotImplementedException($"No idea how to parse this piece: '{c}'"),
    };

    public static Piece ToPiece(this char c)
    {
        var type = ToPieceType(char.ToUpperInvariant(c));

        var color = char.IsUpper(c) ? Piece.White : Piece.None;

        return type | color;
    }

    public static IEnumerable<Piece> Colors => new[] { Piece.White, Piece.None };
    
    public static IEnumerable<Piece> Types => new[] { Piece.Pawn, Piece.Rook, Piece.Knight, Piece.Bishop, Piece.Queen, Piece.King };
    
    public static IEnumerable<Piece> AllPieces => Colors.Select(c => Types.Select(t => c | t)).SelectMany(p => p);
}