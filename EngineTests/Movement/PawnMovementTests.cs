using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Engine.Movement.Tests
{
    [TestClass()]
    public class PawnMovementTests
    {
        [TestMethod]
        public void GetTargetVectorsWhiteStartingPositionTest()
        {
            var a2 = Square.FromAlgebraicNotation("a2");

            var targets = new List<Target>();

            foreach (var vector in PawnMovement.GetTargetVectors(a2, PieceColour.White))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(3, targets.Count);
            Assert.IsTrue(targets.Where(m => m.Square.File == a2.File).All(m => m.Flags.HasFlag(SpecialMove.CannotTake)));
        }

        [TestMethod]
        public void GetTargetVectorsBlackStartingPositionTest()
        {
            var a7 = Square.FromAlgebraicNotation("a7");

            var targets = new List<Target>();

            foreach (var vector in PawnMovement.GetTargetVectors(a7, PieceColour.Black))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(3, targets.Count);
        }

        [TestMethod]
        public void GetTargetVectorsWhitePromotePositionTest()
        {
            var b7 = Square.FromAlgebraicNotation("b7");

            var targets = new List<Target>();

            foreach (var vector in PawnMovement.GetTargetVectors(b7, PieceColour.White))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(3, targets.Count);
            Assert.IsTrue(targets.All(m => m.Flags.HasFlag(SpecialMove.Promote)));
            Assert.IsTrue(targets.Where(m => m.Square.File != b7.File).All(m => m.Flags.HasFlag(SpecialMove.MustTake)));
        }

        [TestMethod]
        public void GetTargetVectorsBlackPromotePositionTest()
        {
            var b2 = Square.FromAlgebraicNotation("b2");

            var targets = new List<Target>();

            foreach (var vector in PawnMovement.GetTargetVectors(b2, PieceColour.Black))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(3, targets.Count);
            Assert.IsTrue(targets.All(m => m.Flags.HasFlag(SpecialMove.Promote)));
            Assert.IsTrue(targets.Where(m => m.Square.File != b2.File).All(m => m.Flags.HasFlag(SpecialMove.MustTake)));
        }


        [TestMethod]
        public void GetTargetVectorsWhiteEnPassantTest()
        {
            var c5 = Square.FromAlgebraicNotation("c5");

            var targets = new List<Target>();

            foreach (var vector in PawnMovement.GetTargetVectors(c5, PieceColour.White))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(5, targets.Count);
            Assert.IsTrue(targets.Count(m => m.Flags.HasFlag(SpecialMove.EnPassant)) == 2);
            Assert.IsTrue(targets.Count(m => m.Flags.HasFlag(SpecialMove.MustTake)) == 2);
        }

        [TestMethod]
        public void GetTargetVectorsBlackEnPassantTest()
        {
            var d4 = Square.FromAlgebraicNotation("d4");

            var targets = new List<Target>();

            foreach (var vector in PawnMovement.GetTargetVectors(d4, PieceColour.Black))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(5, targets.Count);
            Assert.IsTrue(targets.Count(m => m.Flags.HasFlag(SpecialMove.EnPassant)) == 2);
            Assert.IsTrue(targets.Count(m => m.Flags.HasFlag(SpecialMove.MustTake)) == 2);
        }
    }
}