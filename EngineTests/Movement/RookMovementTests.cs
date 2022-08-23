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
            var a1 = Square.FromAlgebraicNotation("b2");

            var rookMovement = new RookMovement();

            var moves = new List<Move>();

            foreach (var vector in rookMovement.GetTargetVectors(a1))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(14, moves.Count);
        }

        [TestMethod()]
        public void GetTargetVectorsCornerCaseTest()
        {
            var a1 = Square.FromAlgebraicNotation("a1");

            var rookMovement = new RookMovement();

            var moves = new List<Move>();

            foreach (var vector in rookMovement.GetTargetVectors(a1))
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

            var rookMovement = new RookMovement();

            var moves = new List<Move>();

            foreach (var vector in rookMovement.GetTargetVectors(a1))
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