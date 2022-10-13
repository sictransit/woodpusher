using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Common.Movement;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Tests.Movement
{
    public abstract class MovementTests
    {
        protected static void AssertAmountOfLegalMoves(Piece pieceType, Piece pieceColor, string square, int count)
        {
            var piece = (pieceType | pieceColor).SetSquare(new Square(square));

            var moves = pieceType switch
            {
                Piece.Pawn => PawnMovement.GetTargetVectors(piece),
                Piece.Knight => KnightMovement.GetTargetVectors(piece),
                Piece.Bishop => BishopMovement.GetTargetVectors(piece),
                Piece.Rook => RookMovement.GetTargetVectors(piece),
                Piece.Queen => QueenMovement.GetTargetVectors(piece),
                Piece.King => KingMovement.GetTargetVectors(piece),
                _ => throw new NotImplementedException(pieceType.ToString()),
            };

            Assert.AreEqual(count, moves.SelectMany(m => m).Count());
        }
    }
}
