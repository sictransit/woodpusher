using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
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
        [TestInitialize]
        public void Initialize()
        {
            Logging.EnableUnitTestLogging(Serilog.Events.LogEventLevel.Information);
        }

        [TestMethod]
        public void ParseSetupTest()
        {
            var board = Board.StartingPosition();

            Assert.AreEqual(Piece.White, board.Counters.ActiveColor);
            Assert.AreEqual(Castlings.WhiteKingside | Castlings.WhiteQueenside | Castlings.BlackKingside | Castlings.BlackQueenside, board.Counters.Castlings);
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

            Assert.AreEqual(Piece.None, board.Counters.ActiveColor);
            Assert.AreEqual(Castlings.None, board.Counters.Castlings);
            Assert.AreEqual(new Square("a3").ToMask(), board.Counters.EnPassantTarget);

            var whiteQueens = board.GetPieces(Piece.White, Piece.Queen);
            var blackRooks = board.GetPieces(Piece.None, Piece.Rook);
            Assert.IsTrue(whiteQueens.Any(p => p.GetSquare().Equals(new Square("c7"))));
            Assert.IsTrue(blackRooks.Any(p => p.GetSquare().Equals(new Square("h5"))));
            Assert.AreEqual(0, board.Counters.HalfmoveClock);
            Assert.AreEqual(34, board.Counters.FullmoveNumber);
        }

        [TestMethod]
        public void ExportStartingPositionTest()
        {
            var board = Board.StartingPosition();

            var export = ForsythEdwardsNotation.Export(board);

            Log.Information(export);

            Assert.AreEqual(ForsythEdwardsNotation.StartingPosition, export);
        }

        [TestMethod]
        public void ExportMagnusCarlsenTest()
        {
            var fen = "5r2/2Q2n2/5k2/7r/P3P1p1/1B6/5P2/6K1 b - a3 0 34";

            var board = ForsythEdwardsNotation.Parse(fen);

            var export = ForsythEdwardsNotation.Export(board);

            Log.Information(export);

            Assert.AreEqual(fen, export);
        }
    }
}