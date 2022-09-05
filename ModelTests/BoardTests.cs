using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Diagnostics;

namespace SicTransit.Woodpusher.Tests
{
    [TestClass()]
    public class BoardTests
    {
        [TestMethod()]
        public void BoardTest()
        {
            var board = new Board();

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

            Assert.IsFalse(board.IsOccupied(e1));
            Assert.IsFalse(board.IsOccupied(e8));
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
                    var square = new Square(f, r);

                    board = board.AddPiece(square, whitePawn);

                    Assert.AreEqual(whitePawn, board.Get(square));
                }
            }
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

        [TestMethod]
        public void GetPositionsOnFileTest()
        {
            Board board = new Board();

            var pieces = new HashSet<Piece>();

            var file = 3;
            var rank = 0;

            foreach (PieceType pieceType in Enum.GetValues(typeof(PieceType)))
            {
                var piece = new Piece(pieceType, PieceColour.Black);
                pieces.Add(piece);

                board = board.AddPiece(new Square(file, rank++), piece);
            }

            foreach (var position in board.GetPositionsOnFile(PieceColour.Black, file))
            {
                Assert.IsTrue(pieces.Contains(position.Piece));
            }

            Assert.AreEqual(0, board.GetPositionsOnFile(PieceColour.White, file).Count());
        }

        [TestMethod]
        public void PlayTest()
        {
            var whiteBishop = new Piece(PieceType.Bishop, PieceColour.White);
            var blackPawn = new Piece(PieceType.Pawn, PieceColour.Black);

            var c1 = Square.FromAlgebraicNotation("c1");
            var g7 = Square.FromAlgebraicNotation("g7");

            var board = new Board().AddPiece(c1, whiteBishop).AddPiece(g7, blackPawn);

            Assert.AreEqual(0, board.Counters.HalfmoveClock);
            Assert.AreEqual(0, board.Counters.FullmoveNumber);
            Assert.AreEqual(PieceColour.White, board.Counters.ActiveColour);
            Assert.IsNull(board.Counters.EnPassantTarget);

            Trace.WriteLine(board.PrettyPrint());

            var d2 = Square.FromAlgebraicNotation("d2");

            board = board.Play(new(new(whiteBishop, c1), new(d2)));
            Assert.AreEqual(PieceColour.Black, board.Counters.ActiveColour);
            Assert.AreEqual(1, board.Counters.HalfmoveClock);
            Assert.AreEqual(0, board.Counters.FullmoveNumber);

            Trace.WriteLine(board.PrettyPrint());

            var g5 = Square.FromAlgebraicNotation("g5");

            board = board.Play(new(new(blackPawn, g7), new(g5, SpecialMove.CannotTake, Square.FromAlgebraicNotation("g6"))));
            Assert.AreEqual(PieceColour.White, board.Counters.ActiveColour);
            Assert.AreEqual(Square.FromAlgebraicNotation("g6"), board.Counters.EnPassantTarget);
            Assert.AreEqual(0, board.Counters.HalfmoveClock);
            Assert.AreEqual(1, board.Counters.FullmoveNumber);

            Trace.WriteLine(board.PrettyPrint());

            board = board.Play(new(new(whiteBishop, d2), new(g5)));
            Assert.IsNull(board.Counters.EnPassantTarget);
            Assert.AreEqual(0, board.Counters.HalfmoveClock);
            Assert.AreEqual(1, board.Counters.FullmoveNumber);

            Assert.AreEqual(0, board.GetPositions(PieceColour.Black).Count());
            Assert.AreEqual(1, board.GetPositions(PieceColour.White).Count());

            Assert.AreEqual(whiteBishop, board.Get(g5));

            Trace.WriteLine(board.PrettyPrint());
        }
    }
}