using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SicTransit.Woodpusher.Model.Tests
{
    [TestClass()]
    public class SquareTests
    {
        [TestMethod]
        public void FromAlgebraicNotationA1Test()
        {
            var p1 = new Square(0, 0);
            var p2 = new Square("a1");

            Assert.AreEqual(p1, p2);
        }

        [TestMethod]
        public void FromAlgebraicNotationH8Test()
        {
            var p1 = new Square(7, 7);
            var p2 = new Square("h8");

            Assert.AreEqual(p1, p2);
        }

    }
}