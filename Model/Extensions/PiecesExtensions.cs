using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model.Extensions;

public static class PiecesExtensions
{
    private const uint SquareMask = 0x00ff;
    private const uint PieceMask = 0xff00;

    public static Pieces SetMask(this Pieces p, ulong mask) => (Pieces)(((uint)p & PieceMask) | (mask << 1));

    public static ulong GetMask(this Pieces p) => ((uint)p & SquareMask) >> 1;

    public static Pieces SetPiece(this Pieces p, Pieces piece) => (Pieces)((uint)p & SquareMask) | piece;

    public static Pieces GetPiece(this Pieces p) => (Pieces)((uint)p & PieceMask);
}