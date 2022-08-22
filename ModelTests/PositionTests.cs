using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Tests
{
    [TestClass()]
    public class PositionTests
    {
        [TestMethod()]
        public void FromAlgebraicNotationA1Test()
        {
            var p1 = new Position(0, 0);
            var p2 = Position.FromAlgebraicNotation("a1");

            Assert.AreEqual(p1, p2);
        }

        [TestMethod()]
        public void FromAlgebraicNotationH8Test()
        {
            var p1 = new Position(7, 7);
            var p2 = Position.FromAlgebraicNotation("h8");

            Assert.AreEqual(p1, p2);
        }

    }
}