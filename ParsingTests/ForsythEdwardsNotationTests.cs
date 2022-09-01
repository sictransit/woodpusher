using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Parsing.Tests
{
    [TestClass()]
    public class ForsythEdwardsNotationTests
    {
        [TestMethod()]
        public void ParseSetupTest()
        {
            var fen = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);

            Assert.AreEqual(Piece.White, fen.ActiveColour);
            Assert.AreEqual(Castlings.WhiteKingside | Castlings.WhiteQueenside | Castlings.BlackKingside | Castlings.BlackQueenside, fen.Castlings);
            Assert.IsNull(fen.EnPassantTarget);
            Assert.AreEqual(0, fen.HalfmoveClock);
            Assert.AreEqual(1, fen.FullmoveNumber);

            var board = fen.Board;

            Trace.WriteLine(BoardExtensions.PrettyPrint(board));

            Assert.AreEqual(Piece.White | Piece.Rook, board.Get(Square.FromAlgebraicNotation("a1")));
        }

        [TestMethod()]
        public void ParseMagnusCarlsenTest()
        {
            var fen = ForsythEdwardsNotation.Parse("5r2/2Q2n2/5k2/7r/P3P1p1/1B6/5P2/6K1 b - a3 0 34");

            Trace.WriteLine(BoardExtensions.PrettyPrint(fen.Board));

            Assert.AreEqual(Piece.Black, fen.ActiveColour);
            Assert.AreEqual(Castlings.None, fen.Castlings);
            Assert.AreEqual(Square.FromAlgebraicNotation("a3"), fen.EnPassantTarget);
            Assert.AreEqual(Piece.White | Piece.Queen, fen.Board.Get(Square.FromAlgebraicNotation("c7")));
            Assert.AreEqual(Piece.Black | Piece.Rook, fen.Board.Get(Square.FromAlgebraicNotation("h5")));
            Assert.AreEqual(0, fen.HalfmoveClock);
            Assert.AreEqual(34, fen.FullmoveNumber);
        }
    }
}