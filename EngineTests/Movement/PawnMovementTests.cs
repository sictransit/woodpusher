using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Engine.Movement;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Engine.Movement.Tests
{
    [TestClass()]
    public class PawnMovementTests
    {
        [TestMethod()]
        public void GetTargetVectorsStartingPositionTest()
        {
            var a2 = Square.FromAlgebraicNotation("a2");

            var moves = new List<Move>();

            foreach (var vector in PawnMovement.GetTargetVectors(a2, PieceColour.White))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(4, moves.Count);
        }

        [TestMethod()]
        public void GetTargetVectorsPromotePositionTest()
        {
            var b7 = Square.FromAlgebraicNotation("b7");

            var moves = new List<Move>();

            foreach (var vector in PawnMovement.GetTargetVectors(b7, PieceColour.White))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(3, moves.Count);
            Assert.IsTrue(moves.All(m => (m.Flags & MovementFlags.Promote) != 0));
        }

    }
}