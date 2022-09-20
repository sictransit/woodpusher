using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Engine;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Tests
{
    [TestClass()]
    public class PatzerTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Logging.EnableUnitTestLogging(Serilog.Events.LogEventLevel.Debug);
        }

        [TestMethod]
        public void InitializeTest()
        {
            var patzer = new Patzer();

            Assert.IsNotNull(patzer.Board);

            Assert.AreEqual(PieceColor.White, patzer.Board.ActiveColor);

            Assert.AreEqual(Castlings.Kingside | Castlings.Queenside, patzer.Board.Counters.WhiteCastlings);
            Assert.AreEqual(Castlings.Kingside | Castlings.Queenside, patzer.Board.Counters.BlackCastlings);
        }

        [TestMethod()]
        public void PlayBestMoveTest()
        {
            var patzer = new Patzer();

            var bestMove = patzer.PlayBestMove();

            Assert.IsNotNull(bestMove);

            Assert.AreEqual(1, bestMove.From.Rank);
        }

        [TestMethod()]
        public void PlayMultipleBestMoveTest()
        {
            var patzer = new Patzer();

            for (var i = 0; i < 10; i++)
            {
                var bestMove = patzer.PlayBestMove();

                Assert.IsNotNull(bestMove);
            }
        }
    }
}