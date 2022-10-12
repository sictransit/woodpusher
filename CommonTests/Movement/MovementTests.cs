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
            var position = (pieceType| pieceColor).SetSquare(new Square(square));
            
            IEnumerable<IEnumerable<Move>> moves = pieceType switch
            {
                Pieces.Pawn => PawnMovement.GetTargetVectors(position),
                Pieces.Knight => KnightMovement.GetTargetVectors(position),
                Pieces.Bishop => BishopMovement.GetTargetVectors(position),
                Pieces.Rook => RookMovement.GetTargetVectors(position),
                Pieces.Queen => QueenMovement.GetTargetVectors(position),
                Pieces.King => KingMovement.GetTargetVectors(position),
                _ => throw new NotImplementedException(pieceType.ToString()),
            };
            
            Assert.AreEqual(count, moves.SelectMany(m => m).Count());
        }
    }
}
