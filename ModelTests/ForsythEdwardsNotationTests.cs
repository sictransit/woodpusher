using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Diagnostics;

namespace SicTransit.Woodpusher.Tests
{
    [TestClass()]
    public class ForsythEdwardsNotationTests
    {
        [TestMethod()]
        public void ParseSetupTest()
        {
            var startingPosition = @"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

            var fen = ForsythEdwardsNotation.Parse(startingPosition);

            Assert.AreEqual(PieceColour.White, fen.ActiveColour);
            Assert.AreEqual(Castlings.WhiteKingside | Castlings.WhiteQueenside | Castlings.BlackKingside | Castlings.BlackQueenside, fen.Castlings);
            Assert.IsNull(fen.EnPassantTarget);
            Assert.AreEqual(0, fen.HalfmoveClock);
            Assert.AreEqual(1, fen.FullmoveNumber);

            var board = fen.Board;

            Trace.WriteLine(BoardExtensions.PrettyPrint(board));

            Assert.AreEqual(new Piece(PieceColour.White, PieceType.Rook), board.Get(Position.FromAlgebraicNotation("a1")));
        }

        [TestMethod()]
        public void ParseMagnusCarlsenTest()
        {
            var fen = ForsythEdwardsNotation.Parse("5r2/2Q2n2/5k2/7r/P3P1p1/1B6/5P2/6K1 b - a3 0 34");

            Assert.AreEqual(PieceColour.Black, fen.ActiveColour);
            Assert.AreEqual(Castlings.None, fen.Castlings);
            Assert.AreEqual(Position.FromAlgebraicNotation("a3"), fen.EnPassantTarget);
            Assert.AreEqual(0, fen.HalfmoveClock);
            Assert.AreEqual(34, fen.FullmoveNumber);

        }
    }
}