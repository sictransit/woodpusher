using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Tests.Movement
{
    [TestClass()]
    public class RookMovementTests : MovementTests
    {
        [TestMethod]
        public void GetTargetVectorsTest()
        {
            AssertAmountOfLegalMoves(PieceType.Rook, PieceColor.White, "b2", 14);
        }

        [TestMethod]
        public void GetTargetVectorsCornerCaseTest()
        {
            AssertAmountOfLegalMoves(PieceType.Rook, PieceColor.White, "a1", 14);
        }
    }
}