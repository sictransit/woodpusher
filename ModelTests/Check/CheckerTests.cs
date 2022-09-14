using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Check;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Tests.Check
{
    [TestClass()]
    public class CheckerTests
    {
        [TestMethod()]
        public void KingAtE1Test()
        {
            var checker = new Checker();

            var checkMask = checker.GetCheckMask(PieceColour.White, new Square("e1"));

            var pawnSquares = checkMask.Pawn.ToSquares().ToArray();
            Assert.AreEqual(2, pawnSquares.Length);
            Assert.IsTrue(pawnSquares.Contains(new Square("d2")));
            Assert.IsTrue(pawnSquares.Contains(new Square("f2")));

            var knightSquares = checkMask.Knight.ToSquares().ToArray();
            Assert.AreEqual(4, knightSquares.Length);
            Assert.IsTrue(knightSquares.Contains(new Square("c2")));
            Assert.IsTrue(knightSquares.Contains(new Square("d3")));
            Assert.IsTrue(knightSquares.Contains(new Square("f3")));
            Assert.IsTrue(knightSquares.Contains(new Square("g2")));

            Assert.AreEqual(21, checkMask.Queen.ToSquares().Count());
            Assert.AreEqual(14, checkMask.Rook.ToSquares().Count());
            Assert.AreEqual(7, checkMask.Bishop.ToSquares().Count());
        }
    }
}