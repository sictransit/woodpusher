﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Engine;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Parsing;

namespace SicTransit.Woodpusher.Tests
{
    [TestClass()]
    public class PatzerTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Logging.EnableUnitTestLogging(Serilog.Events.LogEventLevel.Debug);
        }

        [TestMethod]
        public void InitializeTest()
        {
            var patzer = new Patzer();

            Assert.IsNotNull(patzer.Board);

            Assert.AreEqual(PieceColor.White, patzer.Board.ActiveColor);

            Assert.AreEqual(Castlings.Kingside | Castlings.Queenside, patzer.Board.Counters.WhiteCastlings);
            Assert.AreEqual(Castlings.Kingside | Castlings.Queenside, patzer.Board.Counters.BlackCastlings);
        }

        [TestMethod()]
        public void PlayBestMoveTest()
        {
            var patzer = new Patzer();

            var bestMove = patzer.FindBestMove();

            Assert.IsNotNull(bestMove);

            Assert.AreEqual(1, bestMove.From.Rank);
        }

        [TestMethod()]
        public void PlayMultipleBestMoveTest()
        {
            var patzer = new Patzer();

            var moves = new List<AlgebraicMove>();

            for (var i = 0; i < 10; i++)
            {
                var move = patzer.FindBestMove();

                Assert.IsNotNull(move);
                
                moves.Add(move);

                patzer.Position(ForsythEdwardsNotation.StartingPosition, moves);
            }
        }

        [TestMethod()]
        public void TakeTheQueenTest()
        {
 //+---+---+---+---+---+---+---+---+
 //| Q |   |   | K |   |   |   | k | 8
 //+---+---+---+---+---+---+---+---+
 //|   |   |   |   |   |   |   |   | 7
 //+---+---+---+---+---+---+---+---+
 //|   |   | p |   |   |   |   |   | 6
 //+---+---+---+---+---+---+---+---+
 //|   |   |   | b |   |   |   |   | 5
 //+---+---+---+---+---+---+---+---+
 //|   | p |   |   |   |   |   |   | 4
 //+---+---+---+---+---+---+---+---+
 //|   | n | P |   |   |   |   |   | 3
 //+---+---+---+---+---+---+---+---+
 //| q | q |   |   |   |   |   |   | 2
 //+---+---+---+---+---+---+---+---+
 //|   |   |   |   |   | r |   |   | 1
 //+---+---+---+---+---+---+---+---+
 //  a   b   c   d   e   f   g   h

            var patzer = new Patzer();

            patzer.Position("Q2K3k/8/2p5/3b4/1p6/1nP5/qq6/5r2 b - - 0 93", Enumerable.Empty<AlgebraicMove>());

            var bestMove = patzer.FindBestMove();

            Assert.AreEqual("a2a8", bestMove.Notation);
        }

        [TestMethod]
        public void FindMateInOneTest()
        {
            var patzer = new Patzer();
            patzer.Position("k7/8/1QP5/8/8/8/8/7K w - - 0 1", Enumerable.Empty<AlgebraicMove>());

            var bestMove = patzer.FindBestMove();

            Assert.AreEqual("b6b7", bestMove.Notation);
        }
    }


}