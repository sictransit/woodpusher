using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Tests.Movement
{
    [TestClass()]
    public class PawnMovementTests : MovementTests
    {
        [TestMethod]
        public void GetTargetVectorsWhiteStartingPositionTest()
        {
            AssertAmountOfLegalMoves(Pieces.Pawn, Pieces.White, "a2", 3);
        }

        [TestMethod]
        public void GetTargetVectorsBlackStartingPositionTest()
        {
            AssertAmountOfLegalMoves(Pieces.Pawn, Pieces.None, "a7", 3);
        }

        [TestMethod]
        public void GetTargetVectorsWhitePromotePositionTest()
        {
            AssertAmountOfLegalMoves(Pieces.Pawn, Pieces.White, "b7", 12);
        }

        [TestMethod]
        public void GetTargetVectorsBlackPromotePositionTest()
        {
            AssertAmountOfLegalMoves(Pieces.Pawn, Pieces.None, "b2", 12);
        }


        [TestMethod]
        public void GetTargetVectorsWhiteEnPassantTest()
        {
            AssertAmountOfLegalMoves(Pieces.Pawn, Pieces.White, "c5", 5);
        }

        [TestMethod]
        public void GetTargetVectorsBlackEnPassantTest()
        {
            AssertAmountOfLegalMoves(Pieces.Pawn, Pieces.None, "d4", 5);
        }
    }
}