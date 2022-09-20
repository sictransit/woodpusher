using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model.Lookup
{
    public class Masks
    {
        public static readonly ulong CenterMask =
            new[] { new Square("d4"), new Square("d5"), new Square("e4"), new Square("e5") }.ToMask();

        public static readonly Square WhiteKingsideRookSquare = new("h1");
        public static readonly Square WhiteQueensideRookSquare = new("a1");
        public static readonly Square BlackKingsideRookSquare = new("h8");
        public static readonly Square BlackQueensideRookSquare = new("a8");

        public static readonly Position WhiteKingsideRook =
            new(new Piece(PieceType.Rook, PieceColor.White), WhiteKingsideRookSquare);
        public static readonly Position WhiteQueensideRook =
            new(new Piece(PieceType.Rook, PieceColor.White), WhiteQueensideRookSquare);
        public static readonly Position BlackKingsideRook =
            new(new Piece(PieceType.Rook, PieceColor.Black), BlackKingsideRookSquare);
        public static readonly Position BlackQueensideRook =
            new(new Piece(PieceType.Rook, PieceColor.Black), BlackQueensideRookSquare);

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
