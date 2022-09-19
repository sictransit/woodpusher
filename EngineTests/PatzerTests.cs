using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Common;
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
            Logging.EnableUnitTestLogging(Serilog.Events.LogEventLevel.Debug);

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

        [TestMethod()]
        public void PlayBestMoveTest()
        {
            var bestMove = patzer.PlayBestMove();

            Assert.IsNotNull(bestMove);

            Assert.AreEqual(1, bestMove.From.Rank);
        }

        [TestMethod()]
        public void PlayMultipleBestMoveTest()
        {
            for (int i = 0; i < 10; i++)
            {
                var bestMove = patzer.PlayBestMove();

                Assert.IsNotNull(bestMove);
            }
        }
    }
}