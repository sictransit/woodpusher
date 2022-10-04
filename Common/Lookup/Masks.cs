using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Lookup
{
    public class Masks
    {
        public static readonly ulong WhiteKingsideRookStartingSquare = new Square("h1").ToMask();
        public static readonly ulong WhiteQueensideRookStartingSquare = new Square("a1").ToMask();
        public static readonly ulong BlackKingsideRookStartingSquare = new Square("h8").ToMask();
        public static readonly ulong BlackQueensideRookStartingSquare = new Square("a8").ToMask();

        public static readonly ulong WhiteKingsideRookCastlingSquare = new Square("f1").ToMask();
        public static readonly ulong WhiteQueensideRookCastlingSquare = new Square("d1").ToMask();
        public static readonly ulong BlackKingsideRookCastlingSquare = new Square("f8").ToMask();
        public static readonly ulong BlackQueensideRookCastlingSquare = new Square("d8").ToMask();


        public static readonly Position WhiteKingsideRook =
            new(new Piece(PieceType.Rook, PieceColor.White), WhiteKingsideRookStartingSquare.ToSquare());
        public static readonly Position WhiteQueensideRook =
            new(new Piece(PieceType.Rook, PieceColor.White), WhiteQueensideRookStartingSquare.ToSquare());
        public static readonly Position BlackKingsideRook =
            new(new Piece(PieceType.Rook, PieceColor.Black), BlackKingsideRookStartingSquare.ToSquare());
        public static readonly Position BlackQueensideRook =
            new(new Piece(PieceType.Rook, PieceColor.Black), BlackQueensideRookStartingSquare.ToSquare());

    }
}
