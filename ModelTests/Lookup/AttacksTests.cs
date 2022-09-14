using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Model.Lookup;

namespace SicTransit.Woodpusher.Tests.Lookup
{
    [TestClass()]
    public class AttacksTests
    {
        [TestMethod()]
        public void WhiteKingAtE1Test()
        {
            var attacks = new Attacks();

            var threats = attacks.GetThreatMask(PieceColour.White, new Square("e1"));

            var pawnSquares = threats.PawnMask.ToSquares().ToArray();
            Assert.AreEqual(2, pawnSquares.Length);
            Assert.IsTrue(pawnSquares.Contains(new Square("d2")));
            Assert.IsTrue(pawnSquares.Contains(new Square("f2")));

            var knightSquares = threats.KnightMask.ToSquares().ToArray();
            Assert.AreEqual(4, knightSquares.Length);
            Assert.IsTrue(knightSquares.Contains(new Square("c2")));
            Assert.IsTrue(knightSquares.Contains(new Square("d3")));
            Assert.IsTrue(knightSquares.Contains(new Square("f3")));
            Assert.IsTrue(knightSquares.Contains(new Square("g2")));

            Assert.AreEqual(21, threats.QueenMask.ToSquares().Count());
            Assert.AreEqual(14, threats.RookMask.ToSquares().Count());
            Assert.AreEqual(7, threats.BishopMask.ToSquares().Count());
            Assert.AreEqual(5, threats.KingMask.ToSquares().Count());
        }

        [TestMethod()]
        public void WhiteKingAtE8Test()
        {
            var attacks = new Attacks();

            var threats = attacks.GetThreatMask(PieceColour.White, new Square("e8"));
            
            Assert.AreEqual(0, threats.PawnMask.ToSquares().Count());            
            Assert.AreEqual(4, threats.KnightMask.ToSquares().Count());
            Assert.AreEqual(21, threats.QueenMask.ToSquares().Count());
            Assert.AreEqual(14, threats.RookMask.ToSquares().Count());
            Assert.AreEqual(7, threats.BishopMask.ToSquares().Count());
            Assert.AreEqual(5, threats.KingMask.ToSquares().Count());
        }

    }
}