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
    public class QueenMovementTests
    {
        [TestMethod()]
        public void GetTargetVectorsTest()
        {
            var b2 = Square.FromAlgebraicNotation("b2");

            var moves = new List<Move>();

            foreach (var vector in QueenMovement.GetTargetVectors(b2))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(23, moves.Count);
        }
    }
}