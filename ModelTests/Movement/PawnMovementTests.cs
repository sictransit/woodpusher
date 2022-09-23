using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Tests.Movement
{
    [TestClass()]
    public class PawnMovementTests : MovementTests
    {
        [TestMethod]
        public void GetTargetVectorsWhiteStartingPositionTest()
        {
            AssertAmountOfLegalMoves(PieceType.Pawn, PieceColor.White, "a2", 3);
        }

        [TestMethod]
        public void GetTargetVectorsBlackStartingPositionTest()
        {
            AssertAmountOfLegalMoves(PieceType.Pawn, PieceColor.Black, "a7", 3);
        }

        [TestMethod]
        public void GetTargetVectorsWhitePromotePositionTest()
        {
            AssertAmountOfLegalMoves(PieceType.Pawn, PieceColor.White, "b7", 12);
        }

        [TestMethod]
        public void GetTargetVectorsBlackPromotePositionTest()
        {
            AssertAmountOfLegalMoves(PieceType.Pawn, PieceColor.Black, "b2", 12);
        }


        [TestMethod]
        public void GetTargetVectorsWhiteEnPassantTest()
        {
            AssertAmountOfLegalMoves(PieceType.Pawn, PieceColor.White, "c5", 5);
        }

        [TestMethod]
        public void GetTargetVectorsBlackEnPassantTest()
        {
            AssertAmountOfLegalMoves(PieceType.Pawn, PieceColor.Black, "d4", 5);
        }
    }
}