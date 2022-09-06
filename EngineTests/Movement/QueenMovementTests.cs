using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine.Movement.Tests
{
    [TestClass()]
    public class QueenMovementTests
    {
        [TestMethod]
        public void GetTargetVectorsTest()
        {
            var b2 = new Square("b2");

            var targets = new List<Target>();

            foreach (var vector in QueenMovement.GetTargetVectors(b2))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(23, targets.Count);
        }
    }
}