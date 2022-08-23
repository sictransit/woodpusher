using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine.Movement.Tests
{
    [TestClass()]
    public class RookMovementTests
    {
        [TestMethod()]
        public void GetAvailableSquaresTest()
        {
            var a1 = Square.FromAlgebraicNotation("b2");

            var rookMovement = new RookMovement();

            var moves = new List<Square>();

            for (int i = 0; i <= rookMovement.Directions; i++)
            {
                var squares = rookMovement.GetTargetSquares(a1, i);

                moves.AddRange(squares);
            }

            Assert.AreEqual(14, moves.Count);
        }

        [TestMethod()]
        public void GetAvailableSquaresCornerCaseTest()
        {
            var a1 = Square.FromAlgebraicNotation("a1");

            var rookMovement = new RookMovement();

            var moves = new List<Square>();

            for (int i = 0; i <= rookMovement.Directions; i++)
            {
                var squares = rookMovement.GetTargetSquares(a1, i);

                moves.AddRange(squares);
            }

            Assert.AreEqual(14, moves.Count);
        }

        [TestMethod()]
        public void GetAvailableSquaresButBlockedTest()
        {
            var a1 = Square.FromAlgebraicNotation("a1");

            var blockAtA3 = Square.FromAlgebraicNotation("a3");

            var rookMovement = new RookMovement();

            var moves = new List<Square>();

            for (int i = 0; i <= rookMovement.Directions; i++)
            {
                foreach (var square in rookMovement.GetTargetSquares(a1, i))
                {
                    if (square.Equals(blockAtA3))
                    {
                        break;
                    }

                    moves.Add(square);
                }
            }

            Assert.AreEqual(8, moves.Count);
        }
    }
}