using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SicTransit.Woodpusher.Model.Tests
{
    [TestClass()]
    public class AlgebraicMoveTests
    {
        [TestMethod]
        public void TryParsePromotionTest()
        {
            var notation = "e7e8q";

            var move = AlgebraicMove.Parse(notation);

            Assert.IsNotNull(move);

            Assert.AreEqual(notation, move.Notation);
        }

        [TestMethod]
        public void TryParseTest()
        {
            var notation = "e7e8";

            var move = AlgebraicMove.Parse(notation);

            Assert.IsNotNull(move);

            Assert.AreEqual(notation, move.Notation);
        }

    }
}