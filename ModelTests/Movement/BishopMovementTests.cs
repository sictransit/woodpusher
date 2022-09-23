using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Movement;

namespace SicTransit.Woodpusher.Tests.Movement
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