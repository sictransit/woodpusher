using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Tests.Movement
{
    [TestClass()]
    public class RookMovementTests : MovementTests
    {
        [TestMethod]
        public void GetTargetVectorsTest()
        {
            AssertAmountOfLegalMoves(Piece.Rook, Piece.White, "b2", 14);
        }

        [TestMethod]
        public void GetTargetVectorsCornerCaseTest()
        {
            AssertAmountOfLegalMoves(Piece.Rook, Piece.White, "a1", 14);
        }
    }
}