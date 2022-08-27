using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SicTransit.Woodpusher.Model.Extensions.Tests
{
    [TestClass()]
    public class SquareExtensionsTests
    {
        [TestMethod()]
        public void ToMaskTest()
        {
            Assert.AreEqual(1ul, Square.FromAlgebraicNotation("a1").ToMask());
            Assert.AreEqual(1ul << 9, Square.FromAlgebraicNotation("b2").ToMask());
            Assert.AreEqual(1ul << 63, Square.FromAlgebraicNotation("h8").ToMask());
        }

        [TestMethod()]
        public void ToSquareTest()
        {
            Assert.AreEqual(Square.FromAlgebraicNotation("a1"), 1ul.ToSquare());
            Assert.AreEqual(Square.FromAlgebraicNotation("b2"), (1ul << 9).ToSquare());
            Assert.AreEqual(Square.FromAlgebraicNotation("h8"), (1ul << 63).ToSquare());
        }
    }
}