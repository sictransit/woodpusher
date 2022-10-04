using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Common.Movement;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Tests.Movement
{
    public abstract class MovementTests
    {
        public static void AssertAmountOfLegalMoves(PieceType pieceType, PieceColor pieceColor, string square, int count)
        {
            var position = new Position(new Piece(pieceType, pieceColor), new Square(square));

            IEnumerable<IEnumerable<Move>> moves;

            switch (pieceType)
            {
                case PieceType.Pawn:
                    moves = PawnMovement.GetTargetVectors(position);
                    break;
                case PieceType.Knight:
                    moves = KnightMovement.GetTargetVectors(position);
                    break;
                case PieceType.Bishop:
                    moves = BishopMovement.GetTargetVectors(position);
                    break;
                case PieceType.Rook:
                    moves = RookMovement.GetTargetVectors(position);
                    break;
                case PieceType.Queen:
                    moves = QueenMovement.GetTargetVectors(position);
                    break;
                case PieceType.King:
                    moves = KingMovement.GetTargetVectors(position);
                    break;
                default:
                    throw new NotImplementedException(pieceType.ToString());

            }

            Assert.AreEqual(count, moves.SelectMany(m => m).Count());
        }
    }
}
