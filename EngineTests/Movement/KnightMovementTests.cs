using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine.Movement.Tests
{
    [TestClass()]
    public class KnightMovementTests
    {
        [TestMethod()]
        public void GetTargetVectorsTest()
        {
            var d4 = Square.FromAlgebraicNotation("d4");

            var targets = new List<Target>();

            foreach (var vector in KnightMovement.GetTargetVectors(d4))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(8, targets.Count);
        }
    }
}