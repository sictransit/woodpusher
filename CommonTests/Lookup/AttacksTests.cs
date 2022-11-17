using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Common.Lookup;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Tests.Lookup
{
    [TestClass()]
    public class AttacksTests
    {
        [TestMethod]
        public void WhiteKingAtE1Test()
        {
            var attacks = new Attacks();

            var threats = attacks.GetThreatMask(Piece.King | Piece.White.SetSquare(new Square("e1")));

            var pawnSquares = threats.Pawn.ToSquares().ToArray();
            Assert.AreEqual(2, pawnSquares.Length);
            Assert.IsTrue(pawnSquares.Contains(new Square("d2")));
            Assert.IsTrue(pawnSquares.Contains(new Square("f2")));

            var knightSquares = threats.Knight.ToSquares().ToArray();
            Assert.AreEqual(4, knightSquares.Length);
            Assert.IsTrue(knightSquares.Contains(new Square("c2")));
            Assert.IsTrue(knightSquares.Contains(new Square("d3")));
            Assert.IsTrue(knightSquares.Contains(new Square("f3")));
            Assert.IsTrue(knightSquares.Contains(new Square("g2")));

            Assert.AreEqual(21, threats.Queen.ToSquares().Count());
            Assert.AreEqual(14, threats.Rook.ToSquares().Count());
            Assert.AreEqual(7, threats.Bishop.ToSquares().Count());
            Assert.AreEqual(5, threats.King.ToSquares().Count());
        }

        [TestMethod]
        public void WhiteKingAtE8Test()
        {
            var attacks = new Attacks();

            var threats = attacks.GetThreatMask(Piece.King | Piece.White.SetSquare(new Square("e8")));

            Assert.AreEqual(0, threats.Pawn.ToSquares().Count());
            Assert.AreEqual(4, threats.Knight.ToSquares().Count());
            Assert.AreEqual(21, threats.Queen.ToSquares().Count());
            Assert.AreEqual(14, threats.Rook.ToSquares().Count());
            Assert.AreEqual(7, threats.Bishop.ToSquares().Count());
            Assert.AreEqual(5, threats.King.ToSquares().Count());
        }

    }
}