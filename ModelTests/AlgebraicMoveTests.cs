using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Tests
{
    [TestClass()]
    public class AlgebraicMoveTests
    {
        [TestMethod()]
        public void TryParsePromotionTest()
        {
            var notation = "e7e8q";

            if (!AlgebraicMove.TryParse(notation, out var move))
            {
                Assert.Fail($"unable to parse: [{notation}]");
            }

            Assert.IsNotNull(move);

            Assert.AreEqual(notation, move.Notation);
        }

        [TestMethod()]
        public void TryParseTest()
        {
            var notation = "e7e8";

            if (!AlgebraicMove.TryParse(notation, out var move))
            {
                Assert.Fail($"unable to parse: [{notation}]");
            }

            Assert.IsNotNull(move);

            Assert.AreEqual(notation, move.Notation);
        }

    }
}