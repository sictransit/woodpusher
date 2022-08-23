using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Engine.Movement;
using SicTransit.Woodpusher.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Engine.Movement.Tests
{
    [TestClass()]
    public class BishopMovementTests
    {
        [TestMethod()]
        public void GetTargetVectorsTest()
        {
            var g7 = Square.FromAlgebraicNotation("g7");

            var bishopMovement = new BishopMovement();

            var moves = new List<Move>();

            foreach (var vector in bishopMovement.GetTargetVectors(g7))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(9, moves.Count);
        }

        [TestMethod()]
        public void GetTargetVectorsCornerCaseTest()
        {
            var a1 = Square.FromAlgebraicNotation("a1");

            var bishopMovement = new BishopMovement();

            var moves = new List<Move>();

            foreach (var vector in bishopMovement.GetTargetVectors(a1))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(7, moves.Count);
            Assert.AreEqual(Square.FromAlgebraicNotation("h8"), moves.Last().Square);
        }
    }
}