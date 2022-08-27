using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine.Movement.Tests
{
    [TestClass()]
    public class RookMovementTests
    {
        [TestMethod()]
        public void GetTargetVectorsTest()
        {
            var b2 = Square.FromAlgebraicNotation("b2");

            var moves = new List<Move>();

            foreach (var vector in RookMovement.GetTargetVectors(b2))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(14, moves.Count);
        }

        [TestMethod()]
        public void GetTargetVectorsCornerCaseTest()
        {
            var a1 = Square.FromAlgebraicNotation("a1");

            var moves = new List<Move>();

            foreach (var vector in RookMovement.GetTargetVectors(a1))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(14, moves.Count);
        }

        [TestMethod()]
        public void GetTargetVectorsBlockedTest()
        {
            var a1 = Square.FromAlgebraicNotation("a1");

            var blockAtA3 = Square.FromAlgebraicNotation("a3");

            var moves = new List<Move>();

            foreach (var vector in RookMovement.GetTargetVectors(a1))
            {
                foreach (var move in vector)
                {
                    if (move.Square.Equals(blockAtA3))
                    {
                        break;
                    }

                    moves.Add(move);

                }
            }

            Assert.AreEqual(8, moves.Count);
        }
    }
}