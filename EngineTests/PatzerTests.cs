using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Engine;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Tests
{
    [TestClass()]
    public class PatzerTests
    {
        private Patzer? patzer;

        [TestInitialize]
        public void Initialize()
        {
            patzer = new Patzer();
        }

        [TestMethod]
        public void InitializeTest()
        {
            Assert.IsNotNull(patzer!.Board);

            Assert.AreEqual(PieceColor.White, patzer.Board.ActiveColor);

            Assert.AreEqual(Castlings.Kingside | Castlings.Queenside, patzer.Board.Counters.WhiteCastlings);
            Assert.AreEqual(Castlings.Kingside | Castlings.Queenside, patzer.Board.Counters.BlackCastlings);
        }
    }
}