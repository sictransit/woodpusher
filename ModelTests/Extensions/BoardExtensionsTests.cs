using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Parsing;

namespace SicTransit.Woodpusher.Tests.Extensions
{
    [TestClass()]
    public class BoardExtensionsTests
    {
        [TestMethod()]
        public void PerftTest()
        {
            var board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);

            Assert.AreEqual(20ul, board.Perft(1));
            Assert.AreEqual(400ul, board.Perft(2));
            Assert.AreEqual(8902ul, board.Perft(3));
            Assert.AreEqual(197281ul, board.Perft(4));
            Assert.AreEqual(4865609ul, board.Perft(5));
            Assert.AreEqual(119060324ul, board.Perft(6));
            Assert.AreEqual(3195901860ul, board.Perft(7));
            Assert.AreEqual(84998978956ul, board.Perft(7));
        }
    }
}