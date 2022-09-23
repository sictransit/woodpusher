using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Movement;
using System.Diagnostics.Metrics;

namespace SicTransit.Woodpusher.Tests.Movement
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