using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Movement;

namespace SicTransit.Woodpusher.Tests.Movement
{
    [TestClass()]
    public class QueenMovementTests : MovementTests
    {
        [TestMethod]
        public void GetTargetVectorsTest()
        {
            AssertAmountOfLegalMoves(PieceType.Queen, PieceColor.White, "b2", 23);
        }
    }
}