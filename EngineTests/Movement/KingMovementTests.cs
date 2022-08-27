using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Engine.Movement.Tests
{
    [TestClass()]
    public class KingMovementTests
    {
        [TestMethod()]
        public void GetTargetVectorsTest()
        {
            var e2 = Square.FromAlgebraicNotation("e2");

            var moves = new List<Move>();

            foreach (var vector in KingMovement.GetTargetVectors(e2, Piece.White))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(8, moves.Count);
        }

        [TestMethod()]
        public void GetTargetVectorsCornerTest()
        {
            foreach (var corner in new[] { "a1", "a8", "h1", "h8" })
            {
                var cornerSquare = Square.FromAlgebraicNotation(corner);

                var moves = new List<Move>();

                foreach (var vector in KingMovement.GetTargetVectors(cornerSquare, Piece.White))
                {
                    moves.AddRange(vector);
                }

                Assert.AreEqual(3, moves.Count);
            }
        }

        [TestMethod()]
        public void GetTargetVectorsWhiteCastlingTest()
        {
            var e1 = Square.FromAlgebraicNotation("e1");

            var moves = new List<Move>();

            foreach (var vector in KingMovement.GetTargetVectors(e1, Piece.White))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(7, moves.Count);
        }

        [TestMethod()]
        public void GetTargetVectorsBlackCastlingTest()
        {
            var e8 = Square.FromAlgebraicNotation("e8");

            var moves = new List<Move>();

            foreach (var vector in KingMovement.GetTargetVectors(e8, Piece.Black))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(7, moves.Count);
        }

    }
}