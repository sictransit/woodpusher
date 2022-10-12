using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Common.Extensions;
using SicTransit.Woodpusher.Common.Parsing;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Diagnostics;

namespace SicTransit.Woodpusher.Common.Tests.Parsing
{
    [TestClass()]
    public class ForsythEdwardsNotationTests
    {
        [TestMethod]
        public void ParseSetupTest()
        {
            var board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);

            Assert.AreEqual(Pieces.White, board.Counters.ActiveColor);
            Assert.AreEqual(Castlings.Kingside | Castlings.Queenside, board.Counters.WhiteCastlings);
            Assert.AreEqual(Castlings.Kingside | Castlings.Queenside, board.Counters.BlackCastlings);
            Assert.AreEqual(0ul, board.Counters.EnPassantTarget);
            Assert.AreEqual(0, board.Counters.HalfmoveClock);
            Assert.AreEqual(1, board.Counters.FullmoveNumber);

            Trace.WriteLine(board.PrettyPrint());
        }

        [TestMethod]
        public void ParseMagnusCarlsenTest()
        {
            var board = ForsythEdwardsNotation.Parse("5r2/2Q2n2/5k2/7r/P3P1p1/1B6/5P2/6K1 b - a3 0 34");

            Trace.WriteLine(board.PrettyPrint());

            Assert.AreEqual(Pieces.None, board.Counters.ActiveColor);
            Assert.AreEqual(Castlings.None, board.Counters.WhiteCastlings);
            Assert.AreEqual(Castlings.None, board.Counters.BlackCastlings);
            Assert.AreEqual(new Square("a3").ToMask(), board.Counters.EnPassantTarget);

            var whiteQueens = board.GetPositions(Pieces.White, Pieces.Queen);
            var blackRooks = board.GetPositions(Pieces.None, Pieces.Rook);
            Assert.IsTrue(whiteQueens.Any(p => p.GetSquare().Equals(new Square("c7"))));
            Assert.IsTrue(blackRooks.Any(p => p.GetSquare().Equals(new Square("h5"))));
            Assert.AreEqual(0, board.Counters.HalfmoveClock);
            Assert.AreEqual(34, board.Counters.FullmoveNumber);
        }
    }
}