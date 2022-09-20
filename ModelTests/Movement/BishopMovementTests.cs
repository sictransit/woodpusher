using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Movement;

namespace SicTransit.Woodpusher.Tests.Movement
{
    [TestClass()]
    public class BishopMovementTests
    {
        [TestMethod]
        public void GetTargetVectorsTest()
        {
            var g7 = new Square("g7");

            var targets = new List<Target>();

            foreach (var vector in BishopMovement.GetTargetVectors(g7))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(9, targets.Count);
        }

        [TestMethod]
        public void GetVectorsFromF1Test()
        {
            var f1 = new Square("f1");

            var targets = new List<Target>();

            foreach (var vector in BishopMovement.GetTargetVectors(f1))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(7, targets.Count);
        }

        [TestMethod]
        public void GetTargetVectorsCornerCaseTest()
        {
            var a1 = new Square("a1");

            var targets = new List<Target>();

            foreach (var vector in BishopMovement.GetTargetVectors(a1))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(7, targets.Count);
            Assert.AreEqual(new Square("h8"), targets.Last().Square);
        }
    }
}