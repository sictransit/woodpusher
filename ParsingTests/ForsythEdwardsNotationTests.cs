using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Parsing;
using System.Diagnostics;

namespace SicTransit.Woodpusher.Tests
{
    [TestClass()]
    public class ForsythEdwardsNotationTests
    {
        [TestMethod]
        public void ParseSetupTest()
        {
            var board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);

            Assert.AreEqual(PieceColour.White, board.Counters.ActiveColour);
            Assert.AreEqual(Castlings.WhiteKingside | Castlings.WhiteQueenside | Castlings.BlackKingside | Castlings.BlackQueenside, board.Counters.Castlings);
            Assert.IsNull(board.Counters.EnPassantTarget);
            Assert.AreEqual(0, board.Counters.HalfmoveClock);
            Assert.AreEqual(1, board.Counters.FullmoveNumber);

            Trace.WriteLine(board.PrettyPrint());

            Assert.AreEqual(new Piece(PieceType.Rook, PieceColour.White), board.Get(Square.FromAlgebraicNotation("a1")));
        }

        [TestMethod]
        public void ParseMagnusCarlsenTest()
        {
            var board = ForsythEdwardsNotation.Parse("5r2/2Q2n2/5k2/7r/P3P1p1/1B6/5P2/6K1 b - a3 0 34");

            Trace.WriteLine(board.PrettyPrint());

            Assert.AreEqual(PieceColour.Black, board.Counters.ActiveColour);
            Assert.AreEqual(Castlings.None, board.Counters.Castlings);
            Assert.AreEqual(Square.FromAlgebraicNotation("a3"), board.Counters.EnPassantTarget);
            Assert.AreEqual(new Piece(PieceType.Queen, PieceColour.White), board.Get(Square.FromAlgebraicNotation("c7")));
            Assert.AreEqual(new Piece(PieceType.Rook, PieceColour.Black), board.Get(Square.FromAlgebraicNotation("h5")));
            Assert.AreEqual(0, board.Counters.HalfmoveClock);
            Assert.AreEqual(34, board.Counters.FullmoveNumber);
        }
    }
}