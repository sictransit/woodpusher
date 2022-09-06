using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Tests
{
    [TestClass()]
    public class SqaureTests
    {
        [TestMethod]
        public void FromAlgebraicNotationA1Test()
        {
            var p1 = new Square(0, 0);
            var p2 = Square.FromAlgebraicNotation("a1");

            Assert.AreEqual(p1, p2);
        }

        [TestMethod]
        public void FromAlgebraicNotationH8Test()
        {
            var p1 = new Square(7, 7);
            var p2 = Square.FromAlgebraicNotation("h8");

            Assert.AreEqual(p1, p2);
        }

    }
}