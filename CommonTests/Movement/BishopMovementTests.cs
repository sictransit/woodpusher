using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Tests.Movement
{
    [TestClass()]
    public class BishopMovementTests : MovementTests
    {
        [TestMethod]
        public void GetTargetVectorsTest()
        {
            AssertAmountOfLegalMoves(PieceType.Bishop, PieceColor.White, "g7", 9);
        }

        [TestMethod]
        public void GetVectorsFromF1Test()
        {
            AssertAmountOfLegalMoves(PieceType.Bishop, PieceColor.White, "f1", 7);
        }

        [TestMethod]
        public void GetTargetVectorsCornerCaseTest()
        {
            AssertAmountOfLegalMoves(PieceType.Bishop, PieceColor.White, "a1", 7);
        }
    }
}