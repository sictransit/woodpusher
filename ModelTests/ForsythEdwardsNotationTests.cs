using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Tests
{
    [TestClass()]
    public class ForsythEdwardsNotationTests
    {
        [TestMethod()]
        public void ParseTest()
        {
            var startingPosition = @"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

            var fen = ForsythEdwardsNotation.Parse(startingPosition);

            Assert.AreEqual(PieceColour.White, fen.ActiveColour);
            Assert.AreEqual(Castlings.WhiteKingside | Castlings.WhiteQueenside | Castlings.BlackKingside | Castlings.BlackQueenside, fen.Castlings);
            Assert.IsNull(fen.EnPassantTarget);
            Assert.AreEqual(0, fen.HalfmoveClock);
            Assert.AreEqual(1, fen.FullmoveNumber);

            var board = fen.Board;

            Assert.AreEqual(new Piece(PieceColour.White, PieceType.Rook), board.Get(Position.FromAlgebraicNotation("a1")));
        }
    }
}