using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Engine.Movement.Tests
{
    [TestClass()]
    public class PawnMovementTests
    {
        [TestMethod()]
        public void GetTargetVectorsWhiteStartingPositionTest()
        {
            var a2 = Square.FromAlgebraicNotation("a2");

            var moves = new List<Move>();

            foreach (var vector in PawnMovement.GetTargetVectors(a2, Piece.White))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(3, moves.Count);
        }

        [TestMethod()]
        public void GetTargetVectorsBlackStartingPositionTest()
        {
            var a7 = Square.FromAlgebraicNotation("a7");

            var moves = new List<Move>();

            foreach (var vector in PawnMovement.GetTargetVectors(a7, Piece.Black))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(3, moves.Count);
        }

        [TestMethod()]
        public void GetTargetVectorsWhitePromotePositionTest()
        {
            var b7 = Square.FromAlgebraicNotation("b7");

            var moves = new List<Move>();

            foreach (var vector in PawnMovement.GetTargetVectors(b7, Piece.White))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(3, moves.Count);
            Assert.IsTrue(moves.All(m => m.Flags.HasFlag(MovementFlags.Promote)));
            Assert.IsTrue(moves.Where(m => m.Square.File != b7.File).All(m => m.Flags.HasFlag(MovementFlags.MustTake)));
        }

        [TestMethod()]
        public void GetTargetVectorsBlackPromotePositionTest()
        {
            var b2 = Square.FromAlgebraicNotation("b2");

            var moves = new List<Move>();

            foreach (var vector in PawnMovement.GetTargetVectors(b2, Piece.Black))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(3, moves.Count);
            Assert.IsTrue(moves.All(m => m.Flags.HasFlag(MovementFlags.Promote)));
            Assert.IsTrue(moves.Where(m => m.Square.File != b2.File).All(m => m.Flags.HasFlag(MovementFlags.MustTake)));
        }


        [TestMethod()]
        public void GetTargetVectorsWhiteEnPassantTest()
        {
            var c5 = Square.FromAlgebraicNotation("c5");

            var moves = new List<Move>();

            foreach (var vector in PawnMovement.GetTargetVectors(c5, Piece.White))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(5, moves.Count);
            Assert.IsTrue(moves.Count(m => m.Flags.HasFlag(MovementFlags.EnPassant)) == 2);
            Assert.IsTrue(moves.Count(m => m.Flags.HasFlag(MovementFlags.MustTake)) == 2);            
        }

        [TestMethod()]
        public void GetTargetVectorsBlackEnPassantTest()
        {
            var d4 = Square.FromAlgebraicNotation("d4");

            var moves = new List<Move>();

            foreach (var vector in PawnMovement.GetTargetVectors(d4, Piece.Black))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(5, moves.Count);
            Assert.IsTrue(moves.Count(m => m.Flags.HasFlag(MovementFlags.EnPassant)) == 2);
            Assert.IsTrue(moves.Count(m => m.Flags.HasFlag(MovementFlags.MustTake)) == 2);
        }
    }
}