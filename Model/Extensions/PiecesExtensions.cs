using SicTransit.Woodpusher.Model.Enums;
using System.Numerics;

namespace SicTransit.Woodpusher.Model.Extensions;

public static class PiecesExtensions
{
    private const int SquareMask = 0x00ff;
    private const int PieceMask = 0xff00;

    public static Pieces SetMask(this Pieces p, ulong mask) => (Pieces)(((int)p & PieceMask) | ( 1+ BitOperations.TrailingZeroCount(mask)));

    public static ulong GetMask(this Pieces p) => 1ul << (((int)p & SquareMask) -1);

    public static Pieces SetPiece(this Pieces p, Pieces piece) => (Pieces)((int)p & SquareMask) | piece;

    public static Pieces GetPiece(this Pieces p) => (Pieces)((int)p & PieceMask);

    public static Pieces GetColor(this Pieces p) => p.Is(Pieces.White) ? Pieces.White : Pieces.Black;

    public static Pieces GetPieceType(this Pieces p) => p.GetPiece() & ~Pieces.White;

    public static Square GetSquare(this Pieces p) => p.GetMask().ToSquare();

    public static Pieces SetSquare(this Pieces p, Square square) => p.SetMask(square.ToMask());

    public static bool Is(this Pieces p, Pieces pieceType) => p.HasFlag(pieceType);

    public static Pieces OpponentColor(this Pieces p) => p.Is(Pieces.White) ? Pieces.Black : Pieces.White;

    public static char ToChar(this Pieces p) 
    {
        var pieceTypes = new[] { (Pieces.Pawn, 'P'), (Pieces.Rook, 'R'), (Pieces.Knight, 'N'), (Pieces.Bishop, 'B'), (Pieces.Queen, 'Q'), (Pieces.King, 'K') };

        foreach (var pieceType in pieceTypes)
        {
            if (p.HasFlag(pieceType.Item1))
            {
                return pieceType.Item2;
;            }
        }

        throw new NotImplementedException(p.ToString());
    }

    public static char ToAlgebraicNotation(this Pieces p) => p.Is(Pieces.White) ? p.ToChar() : char.ToLowerInvariant(p.ToChar());

    public static Pieces ToPieceType(this char c) => c switch
    {
        'P' => Pieces.Pawn,
        'R' => Pieces.Rook,
        'N' => Pieces.Knight,
        'B' => Pieces.Bishop,
        'Q' => Pieces.Queen,
        'K' => Pieces.King,
        _ => throw new NotImplementedException($"No idea how to parse this piece: '{c}'"),
    };


    public static Pieces ToPiece(this char c)
    {
        var type = ToPieceType(char.ToUpperInvariant(c));

        var color = char.IsUpper(c) ? Pieces.White : Pieces.Black;

        return type | color;
    }

}