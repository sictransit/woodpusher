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
            AssertAmountOfLegalMoves(Piece.King, Piece.None, "e2", 8);
        }

        [TestMethod]
        public void GetTargetVectorsCornerTest()
        {
            foreach (var corner in new[] { "a1", "a8", "h1", "h8" })
            {
                AssertAmountOfLegalMoves(Piece.King, Piece.White, corner, 3);
            }
        }

        [TestMethod]
        public void GetTargetVectorsWhiteCastlingTest()
        {
            AssertAmountOfLegalMoves(Piece.King, Piece.White, "e1", 7);
        }

        [TestMethod]
        public void GetTargetVectorsBlackCastlingTest()
        {
            AssertAmountOfLegalMoves(Piece.King, Piece.None, "e8", 7);
        }

    }
}