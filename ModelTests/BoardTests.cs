using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Tests
{
    [TestClass()]
    public class BoardTests
    {
        [TestMethod()]
        public void BoardTest()
        {
            var board = new Board();

            Assert.AreEqual(0u, board.Occupancy);

            var whiteKing = new Piece(PieceColour.White, PieceType.King);
            var blackKing = new Piece(PieceColour.Black, PieceType.King);

            var e1 = Square.FromAlgebraicNotation("e1");

            board.Set(e1, whiteKing);

            Assert.IsTrue(board.White.King > 0);
            Assert.AreEqual(board.White.King, board.Occupancy);

            var e8 = Square.FromAlgebraicNotation("e8");

            board.Set(e8, blackKing);

            Assert.IsTrue(board.Black.King > 0);
            Assert.AreEqual(board.White.King | board.Black.King, board.Occupancy);

            Assert.AreEqual(whiteKing, board.Get(e1));
            Assert.AreEqual(blackKing, board.Get(e8));

            board.Unset(e1, whiteKing);
            board.Unset(e8, blackKing);

            Assert.AreEqual(0u, board.Occupancy);
        }
    }
}