using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Common.Movement;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Tests.Movement
{
    public abstract class MovementTests
    {
        protected static void AssertAmountOfLegalMoves(Pieces pieceType, Pieces pieceColor, string square, int count)
        {
            var piece = (pieceType | pieceColor).SetSquare(new Square(square));

            IEnumerable<IEnumerable<Move>> moves = pieceType switch
            {
                Pieces.Pawn => PawnMovement.GetTargetVectors(piece),
                Pieces.Knight => KnightMovement.GetTargetVectors(piece),
                Pieces.Bishop => BishopMovement.GetTargetVectors(piece),
                Pieces.Rook => RookMovement.GetTargetVectors(piece),
                Pieces.Queen => QueenMovement.GetTargetVectors(piece),
                Pieces.King => KingMovement.GetTargetVectors(piece),
                _ => throw new NotImplementedException(pieceType.ToString()),
            };

            Assert.AreEqual(count, moves.SelectMany(m => m).Count());
        }
    }
}
