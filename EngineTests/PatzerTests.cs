using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Common.Parsing;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using System.Diagnostics;

namespace SicTransit.Woodpusher.Engine.Tests
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

            Assert.AreEqual(Piece.White, patzer.Board.ActiveColor);

            Assert.AreEqual(Castlings.Kingside | Castlings.Queenside, patzer.Board.Counters.WhiteCastlings);
            Assert.AreEqual(Castlings.Kingside | Castlings.Queenside, patzer.Board.Counters.BlackCastlings);
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

            patzer.Position("Q2K3k/8/2p5/3b4/1p6/1nP5/qq6/5r2 b - - 0 93");

            var bestMove = patzer.FindBestMove();

            Assert.AreEqual("a2a8", bestMove.Notation);
        }

        [TestMethod]
        public void FindMateInOneTest()
        {
            var patzer = new Patzer();
            patzer.Position("k7/8/1QP5/8/8/8/8/7K w - - 0 1");

            var bestMove = patzer.FindBestMove();

            Assert.AreEqual("b6b7", bestMove.Notation);
        }

        [TestMethod]
        public void DoNotPlayQueenD2Test()
        {
            var patzer = new Patzer();

            patzer.Position("rnbqkbnr/ppp2ppp/8/4p3/4N3/5N2/PPPP1PPP/R1BQKB1R b KQkq - 0 4");

            var bestMove = patzer.FindBestMove();

            Assert.AreNotEqual("d8d2", bestMove.Notation);
        }

        [TestMethod]
        public void PlayE8F8OrDieTest()
        {
            // e8f8 is a good move; pushing e.g. a B pawn is not 

            //+---+---+---+---+---+---+---+---+
            //| r | n | b | q | k |   | n | r | 8
            //+---+---+---+---+---+---+---+---+
            //| p | p | p |   | b |   |   | p | 7
            //+---+---+---+---+---+---+---+---+
            //|   |   |   |   |   |   | P |   | 6
            //+---+---+---+---+---+---+---+---+
            //|   |   |   | p |   |   |   | Q | 5
            //+---+---+---+---+---+---+---+---+
            //|   |   |   | N | p |   |   |   | 4
            //+---+---+---+---+---+---+---+---+
            //|   |   | N |   |   |   |   |   | 3
            //+---+---+---+---+---+---+---+---+
            //| P | P | P | P |   | P | P | P | 2
            //+---+---+---+---+---+---+---+---+
            //| R |   | B |   | K | B |   | R | 1
            //+---+---+---+---+---+---+---+---+
            //  a   b   c   d   e   f   g   h

            var patzer = new Patzer();

            patzer.Position("rnbqk1nr/ppp1b2p/6P1/3p3Q/3Np3/2N5/PPPP1PPP/R1B1KB1R b KQkq - 0 7");

            var bestMove = patzer.FindBestMove(10000);

            Assert.AreEqual("e8f8", bestMove.Notation);
        }

        [TestMethod]
        public void FindMateInThreeTest()
        {
            var patzer = new Patzer();

            patzer.Position("8/pk6/3B4/1B6/8/P1N5/1Pb2KP1/4R3 w - - 1 38");

            var bestMove = patzer.FindBestMove(10000);

            Assert.AreEqual("e1e7", bestMove.Notation);
        }

        [TestMethod]
        public void QueenG5IsNotAnOption()
        {
            // For some reason it preferred Qg5 before any other move?!            

            //8 r n b q k b n r
            //7 p p p     p p p
            //6
            //5       p p
            //4         P
            //3       P   N
            //2 P P P     P P P
            //1 R N B Q K B   R
            //  A B C D E F G H

            var patzer = new Patzer();

            patzer.Position("rnbqkbnr/ppp2ppp/8/3pp3/4P3/3P1N2/PPP2PPP/RNBQKB1R b KQkq - 0 3");

            var bestMove = patzer.FindBestMove(10000);

            Assert.AreNotEqual("d8g5", bestMove.Notation);
        }

        [TestMethod]
        public void PromotionIsNotTheBestMoveAfterAll()
        {
            // The engine was making no-op moves instead of promoting.
            // ... and it turns out that Stockfish agrees.

            var patzer = new Patzer();

            patzer.Position("6K1/8/1k6/8/6b1/8/6p1/8 b - - 3 156");

            var bestmove = patzer.FindBestMove(10000);

            Assert.IsTrue(bestmove.Notation.Length == 4);
        }

        [TestMethod]
        public void NodeEvaluationTest()
        {
            var patzer = new Patzer();
            patzer.Position("7k/8/8/8/8/1p6/B7/7K w - - 0 1");

            var bestmove = patzer.FindBestMove();

            Assert.AreEqual("a2b3", bestmove.Notation);
        }

        [TestMethod()]
        public void MateInMovesWinningTest()
        {
            var patzer = new Patzer();
            patzer.Position("7k/PP6/8/4K3/8/8/8/8 w - - 0 1");

            var infos = new List<string>();

            void Callback(string s)
            {
                infos.Add(s);
                Trace.WriteLine(s);
            }

            var move = patzer.FindBestMove(5000, Callback);

            Assert.IsNotNull(move);

            Assert.IsTrue(infos.Any(i => i.Contains("mate 3")));
        }

        [TestMethod()]
        public void MateInMovesLosingTest()
        {
            var patzer = new Patzer();
            patzer.Position("7k/PP6/8/4K3/8/8/8/8 b - - 0 1");

            var infos = new List<string>();

            void Callback(string s)
            {
                infos.Add(s);
                Trace.WriteLine(s);
            }

            var move = patzer.FindBestMove(5000, Callback);

            Assert.IsTrue(infos.Any(i => i.Contains("mate -4")));
        }

    }


}