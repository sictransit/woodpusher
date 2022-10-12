using SicTransit.Woodpusher.Model.Enums;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Model.Extensions;

public static class PiecesExtensions
{
    public static Pieces SetMask(this Pieces p, ulong mask)
    {
        return (Pieces)(((uint)p & 0xff00) | (mask << 1));
    }

    public static ulong GetMask(this Pieces p)
    {
        return ((uint)p & 0xff) >> 1;
    }

    public static Pieces SetPiece(this Pieces p, Pieces piece)
    {
        return (Pieces)((uint)p & 0xff) | piece;
    }

    public static Pieces GetPiece(this Pieces p)
    {
        return (Pieces)((uint)p & 0xff00);
    }

}