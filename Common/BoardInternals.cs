using SicTransit.Woodpusher.Common.Lookup;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common
{
    internal class BoardInternals
    {
        public static readonly ulong WhiteKingsideRookStartingSquare = new Square("h1").ToMask();
        public static readonly ulong WhiteQueensideRookStartingSquare = new Square("a1").ToMask();
        public static readonly ulong BlackKingsideRookStartingSquare = new Square("h8").ToMask();
        public static readonly ulong BlackQueensideRookStartingSquare = new Square("a8").ToMask();

        public static readonly ulong WhiteKingsideRookCastlingSquare = new Square("f1").ToMask();
        public static readonly ulong WhiteQueensideRookCastlingSquare = new Square("d1").ToMask();
        public static readonly ulong BlackKingsideRookCastlingSquare = new Square("f8").ToMask();
        public static readonly ulong BlackQueensideRookCastlingSquare = new Square("d8").ToMask();


        public static readonly Piece WhiteKingsideRook =
            (Piece.Rook | Piece.White).SetMask(WhiteKingsideRookStartingSquare);
        public static readonly Piece WhiteQueensideRook =
            (Piece.Rook | Piece.White).SetMask(WhiteQueensideRookStartingSquare);
        public static readonly Piece BlackKingsideRook =
            (Piece.Rook | Piece.None).SetMask(BlackKingsideRookStartingSquare);
        public static readonly Piece BlackQueensideRook =
            (Piece.Rook | Piece.None).SetMask(BlackQueensideRookStartingSquare);

        public static readonly ulong InvalidHash = 0;

        public Attacks Attacks { get; }

        public Scoring Scoring { get; }

        public Moves Moves { get; }

        public Zobrist Zobrist { get; }

        public BoardInternals()
        {
            Attacks = new Attacks();
            Scoring = new Scoring();
            Moves = new Moves();
            Zobrist = new Zobrist();
        }
    }
}
