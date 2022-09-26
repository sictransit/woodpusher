using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model.Lookup
{
    public class Masks
    {
        public static readonly ulong CenterMask =
            new[] { new Square("d4"), new Square("d5"), new Square("e4"), new Square("e5") }.ToMask();

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

        private readonly Dictionary<PieceColor, Dictionary<ulong, ulong>> kingProtectionMasks = new();

        public Masks()
        {
            InitializeKingProtectionMasks();
        }

        private void InitializeKingProtectionMasks()
        {
            foreach (var color in new[] { PieceColor.White, PieceColor.Black })
            {
                kingProtectionMasks.Add(color, new Dictionary<ulong, ulong>());

                for (var shift = 0; shift < 64; shift++)
                {
                    var kingMask = 1ul << shift;
                    var kingSquare = kingMask.ToSquare();

                    var squares = new List<Square>();

                    foreach (var dF in new[] { -1, 0, 1 })
                    {
                        if (Square.TryCreate(kingSquare.File + dF,
                                kingSquare.Rank + (color == PieceColor.White ? 1 : -1), out var square))
                        {
                            squares.Add(square);
                        }
                    }

                    kingProtectionMasks[color].Add(kingMask, squares.ToMask());
                }
            }
        }

        public ulong GetKingProtectionMask(PieceColor color, ulong mask) => kingProtectionMasks[color][mask];
    }
}
