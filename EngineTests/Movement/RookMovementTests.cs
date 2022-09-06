using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine.Movement.Tests
{
    [TestClass()]
    public class RookMovementTests
    {
        [TestMethod]
        public void GetTargetVectorsTest()
        {
            var b2 = Square.FromAlgebraicNotation("b2");

            var targets = new List<Target>();

            foreach (var vector in RookMovement.GetTargetVectors(b2))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(14, targets.Count);
        }

        [TestMethod]
        public void GetTargetVectorsCornerCaseTest()
        {
            var a1 = Square.FromAlgebraicNotation("a1");

            var targets = new List<Target>();

            foreach (var vector in RookMovement.GetTargetVectors(a1))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(14, targets.Count);
        }

        [TestMethod]
        public void GetTargetVectorsBlockedTest()
        {
            var a1 = Square.FromAlgebraicNotation("a1");

            var blockAtA3 = Square.FromAlgebraicNotation("a3");

            var targets = new List<Target>();

            foreach (var vector in RookMovement.GetTargetVectors(a1))
            {
                foreach (var target in vector)
                {
                    if (target.Square.Equals(blockAtA3))
                    {
                        break;
                    }

                    targets.Add(target);

                }
            }

            Assert.AreEqual(8, targets.Count);
        }
    }
}