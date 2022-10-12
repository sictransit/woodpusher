using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Tests.Movement
{
    [TestClass()]
    public class KingMovementTests : MovementTests
    {
        [TestMethod]
        public void GetTargetVectorsTest()
        {
            AssertAmountOfLegalMoves(Pieces.King, Pieces.Black, "e2", 8);
        }

        [TestMethod]
        public void GetTargetVectorsCornerTest()
        {
            foreach (var corner in new[] { "a1", "a8", "h1", "h8" })
            {
                AssertAmountOfLegalMoves(Pieces.King, Pieces.White, corner, 3);
            }
        }

        [TestMethod]
        public void GetTargetVectorsWhiteCastlingTest()
        {
            AssertAmountOfLegalMoves(Pieces.King, Pieces.White, "e1", 7);
        }

        [TestMethod]
        public void GetTargetVectorsBlackCastlingTest()
        {
            AssertAmountOfLegalMoves(Pieces.King, Pieces.Black, "e8", 7);
        }

    }
}