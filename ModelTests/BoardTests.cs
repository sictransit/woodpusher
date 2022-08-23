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

            Assert.AreEqual(0u, board.Aggregate);

            var whiteKing = new Piece(PieceColour.White, PieceType.King);
            var blackKing = new Piece(PieceColour.Black, PieceType.King);

            var e1 = Square.FromAlgebraicNotation("e1");

            board = board.AddPiece(e1, whiteKing);

            Assert.IsTrue(board.White.King > 0);
            Assert.AreEqual(board.White.King, board.Aggregate);

            var e8 = Square.FromAlgebraicNotation("e8");

            board = board.AddPiece(e8, blackKing);

            Assert.IsTrue(board.Black.King > 0);
            Assert.AreEqual(board.White.King | board.Black.King, board.Aggregate);

            Assert.AreEqual(whiteKing, board.Get(e1));
            Assert.AreEqual(blackKing, board.Get(e8));

            board = board.RemovePiece(e1, whiteKing);
            board = board.RemovePiece(e8, blackKing);

            Assert.AreEqual(0u, board.Aggregate);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void OccupiedSquareTest()
        {
            var board = new Board();

            var e1 = Square.FromAlgebraicNotation("e1");
            var whiteKing = new Piece(PieceColour.White, PieceType.King);

            board = board.AddPiece(e1, whiteKing);
            board.AddPiece(e1, whiteKing);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void EmptySquareTest()
        {
            var board = new Board();

            var e1 = Square.FromAlgebraicNotation("e1");
            var whiteKing = new Piece(PieceColour.White, PieceType.King);

            board.RemovePiece(e1, whiteKing);            
        }

        [TestMethod()]
        public void FillTest()
        {
            var board = new Board();

            var whitePawn = new Piece(PieceColour.White, PieceType.Pawn);

            for (int f = 0; f < 8; f++)
            {
                for (int r = 0; r < 8; r++)
                {
                    board = board.AddPiece(new Square(f, r), whitePawn);
                }
            }

            Assert.AreEqual(0xffffffffffffffff, board.Aggregate);
        }
    }
}