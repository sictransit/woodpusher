using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Movement;

namespace SicTransit.Woodpusher.Tests.Movement
{
    [TestClass()]
    public class KingMovementTests
    {
        [TestMethod]
        public void GetTargetVectorsTest()
        {
            var e2 = new Square("e2");

            var targets = new List<Target>();

            foreach (var vector in KingMovement.GetTargetVectors(e2, PieceColor.White))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(8, targets.Count);
        }

        [TestMethod]
        public void GetTargetVectorsCornerTest()
        {
            foreach (var corner in new[] { "a1", "a8", "h1", "h8" })
            {
                var cornerSquare = new Square(corner);

                var targets = new List<Target>();

                foreach (var vector in KingMovement.GetTargetVectors(cornerSquare, PieceColor.White))
                {
                    targets.AddRange(vector);
                }

                Assert.AreEqual(3, targets.Count);
            }
        }

        [TestMethod]
        public void GetTargetVectorsWhiteCastlingTest()
        {
            var e1 = new Square("e1");

            var targets = new List<Target>();

            foreach (var vector in KingMovement.GetTargetVectors(e1, PieceColor.White))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(7, targets.Count);
        }

        [TestMethod]
        public void GetTargetVectorsBlackCastlingTest()
        {
            var e8 = new Square("e8");

            var targets = new List<Target>();

            foreach (var vector in KingMovement.GetTargetVectors(e8, PieceColor.Black))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(7, targets.Count);
        }

    }
}