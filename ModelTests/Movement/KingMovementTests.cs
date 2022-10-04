using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model.Tests.Movement
{
    [TestClass()]
    public class KingMovementTests : MovementTests
    {
        [TestMethod]
        public void GetTargetVectorsTest()
        {
            AssertAmountOfLegalMoves(PieceType.King, PieceColor.Black, "e2", 8);
        }

        [TestMethod]
        public void GetTargetVectorsCornerTest()
        {
            foreach (var corner in new[] { "a1", "a8", "h1", "h8" })
            {
                AssertAmountOfLegalMoves(PieceType.King, PieceColor.White, corner, 3);
            }
        }

        [TestMethod]
        public void GetTargetVectorsWhiteCastlingTest()
        {
            AssertAmountOfLegalMoves(PieceType.King, PieceColor.White, "e1", 7);
        }

        [TestMethod]
        public void GetTargetVectorsBlackCastlingTest()
        {
            AssertAmountOfLegalMoves(PieceType.King, PieceColor.Black, "e8", 7);
        }

    }
}