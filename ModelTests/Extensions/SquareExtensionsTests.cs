using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Tests.Extensions
{
    [TestClass()]
    public class SquareExtensionsTests
    {
        [TestMethod]
        public void ToMaskTest()
        {
            Assert.AreEqual(1ul, new Square("a1").ToMask());
            Assert.AreEqual(1ul << 8, new Square("a2").ToMask());
            Assert.AreEqual(1ul << 9, new Square("b2").ToMask());
            Assert.AreEqual(1ul << 63, new Square("h8").ToMask());
        }

        [TestMethod()]
        public void EnumerableToMaskTest()
        {
            var a1 = new Square("a1");
            var h8 = new Square("h8");

            var mask = a1.ToMask() | h8.ToMask();

            Assert.AreEqual(mask, new[] { a1, h8 }.ToMask());

            Assert.AreEqual(0ul, Enumerable.Empty<Square>().ToMask());
        }


        [TestMethod]
        public void ToSquareTest()
        {
            Assert.AreEqual(new Square("a1"), 1ul.ToSquare());
            Assert.AreEqual(new Square("a2"), (1ul << 8).ToSquare());
            Assert.AreEqual(new Square("b2"), (1ul << 9).ToSquare());
            Assert.AreEqual(new Square("h8"), (1ul << 63).ToSquare());
        }

        [TestMethod()]
        public void ToSquaresTest()
        {
            var a1 = new Square("a1");
            var h8 = new Square("h8");

            var mask = a1.ToMask();
            mask |= h8.ToMask();

            var squares = mask.ToSquares().ToArray();

            Assert.IsTrue(squares.Contains(a1));
            Assert.IsTrue(squares.Contains(h8));
        }

        [TestMethod()]
        public void ToTravelPathTest()
        {
            var a1 = new Square("a1");
            var a2 = new Square("a2");
            var a3 = new Square("a3");
            var a8 = new Square("a8");
            var b2 = new Square("b2");
            var b3 = new Square("b3");
            var f2 = new Square("f2");
            var g2 = new Square("g2");
            var g7 = new Square("g7");
            var h8 = new Square("h8");

            var northEast = a1.ToTravelPath(h8).ToArray();
            Assert.AreEqual(6, northEast.Length);
            Assert.AreEqual(b2, northEast.First());
            Assert.AreEqual(g7, northEast.Last());

            var southWest = h8.ToTravelPath(a1).ToArray();
            Assert.AreEqual(6, southWest.Length);
            Assert.AreEqual(g7, southWest.First());
            Assert.AreEqual(b2, southWest.Last());

            var north = a1.ToTravelPath(a8).ToArray();
            Assert.AreEqual(6, north.Length);
            Assert.AreEqual(a2, north.First());

            var west = g2.ToTravelPath(b2).ToArray();
            Assert.AreEqual(4, west.Length);
            Assert.AreEqual(f2, west.First());

            Assert.AreEqual(0, a2.ToTravelPath(a3).Count());

            Assert.AreEqual(0, a1.ToTravelPath(b3).Count()); // knight
            Assert.AreEqual(0, b3.ToTravelPath(a1).Count()); // knight
        }

        [TestMethod()]
        public void AddFileAndRankToMaskTest()
        {
            var c3 = new Square("c3");

            Assert.AreEqual(new Square("d3"), c3.ToMask().AddFileAndRank(1, 0).ToSquare());
            Assert.AreEqual(new Square("b3"), c3.ToMask().AddFileAndRank(-1, 0).ToSquare());
            Assert.AreEqual(new Square("c4"), c3.ToMask().AddFileAndRank(0, 1).ToSquare());
            Assert.AreEqual(new Square("c2"), c3.ToMask().AddFileAndRank(0, -1).ToSquare());
            Assert.AreEqual(new Square("a1"), c3.ToMask().AddFileAndRank(-2, -2).ToSquare());
            Assert.AreEqual(new Square("a5"), c3.ToMask().AddFileAndRank(-2, 2).ToSquare());
        }
    }
}