using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Lookup
{
    // Reference: https://www.chessprogramming.org/Simplified_Evaluation_Function

    public class Scoring
    {
        public const int PawnValue = 100;
        public const int KnightValue = 320;
        public const int BishopValue = 330;
        public const int RookValue = 500;
        public const int QueenValue = 900;
        public const int KingValue = 20000;

        private readonly Dictionary<Position, int> middleGameEvaluations = new();
        private readonly Dictionary<Position, int> endGameEvaluations = new();

        // pawn

        private static readonly int[] pawnModifiers = new[]
        {
 0,  0,  0,  0,  0,  0,  0,  0,
50, 50, 50, 50, 50, 50, 50, 50,
10, 10, 20, 30, 30, 20, 10, 10,
 5,  5, 10, 25, 25, 10,  5,  5,
 0,  0,  0, 20, 20,  0,  0,  0,
 5, -5,-10,  0,  0,-10, -5,  5,
 5, 10, 10,-20,-20, 10, 10,  5,
 0,  0,  0,  0,  0,  0,  0,  0
        };

        private static readonly int[] knightModifiers = new[]
        {
-50,-40,-30,-30,-30,-30,-40,-50,
-40,-20,  0,  0,  0,  0,-20,-40,
-30,  0, 10, 15, 15, 10,  0,-30,
-30,  5, 15, 20, 20, 15,  5,-30,
-30,  0, 15, 20, 20, 15,  0,-30,
-30,  5, 10, 15, 15, 10,  5,-30,
-40,-20,  0,  5,  5,  0,-20,-40,
-50,-40,-30,-30,-30,-30,-40,-50,
        };

        private static readonly int[] bishopModifiers = new[]
        {
-20,-10,-10,-10,-10,-10,-10,-20,
-10,  0,  0,  0,  0,  0,  0,-10,
-10,  0,  5, 10, 10,  5,  0,-10,
-10,  5,  5, 10, 10,  5,  5,-10,
-10,  0, 10, 10, 10, 10,  0,-10,
-10, 10, 10, 10, 10, 10, 10,-10,
-10,  5,  0,  0,  0,  0,  5,-10,
-20,-10,-10,-10,-10,-10,-10,-20,
        };

        private static readonly int[] rookModifiers = new[]
        {
  0,  0,  0,  0,  0,  0,  0,  0,
  5, 10, 10, 10, 10, 10, 10,  5,
 -5,  0,  0,  0,  0,  0,  0, -5,
 -5,  0,  0,  0,  0,  0,  0, -5,
 -5,  0,  0,  0,  0,  0,  0, -5,
 -5,  0,  0,  0,  0,  0,  0, -5,
 -5,  0,  0,  0,  0,  0,  0, -5,
  0,  0,  0,  5,  5,  0,  0,  0
        };

        private static readonly int[] queenModifiers = new[]
        {
-20,-10,-10, -5, -5,-10,-10,-20,
-10,  0,  0,  0,  0,  0,  0,-10,
-10,  0,  5,  5,  5,  5,  0,-10,
 -5,  0,  5,  5,  5,  5,  0, -5,
  0,  0,  5,  5,  5,  5,  0, -5,
-10,  5,  5,  5,  5,  5,  0,-10,
-10,  0,  5,  0,  0,  0,  0,-10,
-20,-10,-10, -5, -5,-10,-10,-20
        };

        private static readonly int[] kingMiddleGameModifiers = new[]
        {
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-20,-30,-30,-40,-40,-30,-30,-20,
-10,-20,-20,-20,-20,-20,-20,-10,
 20, 20,  0,  0,  0,  0, 20, 20,
 20, 30, 10,  0,  0, 10, 30, 20
        };

        private static readonly int[] kingEndGameModifiers = new[]
        {
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-20,-30,-30,-40,-40,-30,-30,-20,
-10,-20,-20,-20,-20,-20,-20,-10,
 20, 20,  0,  0,  0,  0, 20, 20,
 20, 30, 10,  0,  0, 10, 30, 20
        };

        private static int GetModifierIndex(Position position) => position.Piece.Color == PieceColor.White
                ? position.Square.File + (7 - position.Square.Rank) * 8
                : position.Square.File + position.Square.Rank * 8;

        public Scoring()
        {
            InitializeEvaluations(middleGameEvaluations, false);
            InitializeEvaluations(endGameEvaluations, true);
        }

        private static void InitializeEvaluations(Dictionary<Position, int> evaluations, bool endGame)
        {
            var pieces = new[] { PieceColor.Black, PieceColor.White }.Select(c => new[] { PieceType.Pawn, PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen, PieceType.King }.Select(t => new Piece(t, c))).SelectMany(x => x).ToList();
            var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(0, 8).Select(r => new Square(f, r))).SelectMany(x => x).ToList();
            var positions = pieces.Select(p => squares.Select(s => new Position(p, s))).SelectMany(p => p);

            foreach (var position in positions)
            {
                var modifierIndex = GetModifierIndex(position);
                var evaluation = position.Piece.Type switch
                {
                    PieceType.Pawn => PawnValue + pawnModifiers[modifierIndex],
                    PieceType.Knight => KnightValue + knightModifiers[modifierIndex],
                    PieceType.Bishop => BishopValue + bishopModifiers[modifierIndex],
                    PieceType.Rook => RookValue + rookModifiers[modifierIndex],
                    PieceType.Queen => QueenValue + queenModifiers[modifierIndex],
                    PieceType.King => KingValue + (endGame ? kingEndGameModifiers[modifierIndex] : kingMiddleGameModifiers[modifierIndex]),
                    _ => throw new ArgumentException(position.Piece.Type.ToString()),
                };
                evaluations.Add(position, evaluation);
            }
        }


        public int EvaluatePosition(Position position, bool endGame) => endGame ? endGameEvaluations[position] : middleGameEvaluations[position];
    }
}
