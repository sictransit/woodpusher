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

            var whiteKing = new Piece(PieceType.King, PieceColour.White);
            var blackKing = new Piece(PieceType.King, PieceColour.Black);

            var e1 = Square.FromAlgebraicNotation("e1");

            board = board.AddPiece(e1, whiteKing);

            var e8 = Square.FromAlgebraicNotation("e8");

            board = board.AddPiece(e8, blackKing);

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
            var whiteKing = new Piece(PieceType.King, PieceColour.White);

            board = board.AddPiece(e1, whiteKing);
            board.AddPiece(e1, whiteKing);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void EmptySquareTest()
        {
            var board = new Board();

            var e1 = Square.FromAlgebraicNotation("e1");

            board.RemovePiece(e1, new Piece(PieceType.King, PieceColour.White));
        }

        [TestMethod()]
        public void FillTest()
        {
            var board = new Board();

            var whitePawn = new Piece(PieceType.Pawn, PieceColour.White);

            for (int f = 0; f < 8; f++)
            {
                for (int r = 0; r < 8; r++)
                {
                    board = board.AddPiece(new Square(f, r), whitePawn);
                }
            }

            Assert.AreEqual(0xffffffffffffffff, board.Aggregate);
        }

        [TestMethod]
        public void GetPositionsTest()
        {
            var board = new Board();

            var whiteKing = new Piece(PieceType.King, PieceColour.White);
            var e1 = Square.FromAlgebraicNotation("e1");

            var blackKing = new Piece(PieceType.King, PieceColour.Black);
            var e8 = Square.FromAlgebraicNotation("e8");

            board = board.AddPiece(e1, whiteKing);
            board = board.AddPiece(e8, blackKing);

            var whitePositions = board.GetPositions(PieceColour.White);

            Assert.AreEqual(whiteKing, whitePositions.Single().Piece);
            Assert.AreEqual(e1, whitePositions.Single().Square);

            var blackPositions = board.GetPositions(PieceColour.Black);

            Assert.AreEqual(blackKing, blackPositions.Single().Piece);
            Assert.AreEqual(e8, blackPositions.Single().Square);
        }
    }
}