using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model.Tests.Movement
{
    [TestClass()]
    public class KnightMovementTests : MovementTests
    {
        [TestMethod]
        public void GetTargetVectorsTest()
        {
            AssertAmountOfLegalMoves(PieceType.Knight, PieceColor.White, "d4", 8);
        }
    }
}