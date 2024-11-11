using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Common.Parsing;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher.Engine.Tests
{
    [TestClass()]
    public class PatzerTests
    {
        private Patzer patzer;
        private readonly List<string> traceLines = [];

        private void PatzerCallback(string s)
        {
            Trace.WriteLine(s);
            traceLines.Add(s);
        }

        [TestInitialize]
        public void Initialize()
        {
            Logging.EnableUnitTestLogging(Serilog.Events.LogEventLevel.Debug);

            traceLines.Clear();
            patzer = new Patzer(PatzerCallback);
        }

        [TestMethod]
        public void InitializeTest()
        {
            Assert.IsNotNull(patzer.Board);

            Assert.AreEqual(Piece.White, patzer.Board.ActiveColor);

            Assert.AreEqual(Castlings.WhiteKingside | Castlings.WhiteQueenside | Castlings.BlackKingside | Castlings.BlackQueenside, patzer.Board.Counters.Castlings);

            Assert.AreEqual(0, patzer.Board.Counters.Ply);
        }


        [TestMethod]
        public void PlayMultipleBestMoveTest()
        {
            var moves = new List<AlgebraicMove>();

            for (var i = 0; i < 10; i++)
            {
                var bestMove = patzer.FindBestMove();

                Assert.IsNotNull(bestMove);

                moves.Add(bestMove);

                patzer.Position(ForsythEdwardsNotation.StartingPosition, moves);
            }
        }

        [TestMethod]
        public void TakeTheQueenTest()
        {
            patzer.Position("Q2K3k/8/2p5/3b4/1p6/1nP5/qq6/5r2 b - - 0 93");

            var bestMove = patzer.FindBestMove();

            Assert.AreEqual("a2a8", bestMove.Notation);
        }

        [TestMethod]
        [Ignore("Long running test")]
        public void HardMultipleBestMoveTest()
        {
            var positions = new[]
            {
                "1k1r4/pp1b1R2/3q2pp/4p3/2B5/4Q3/PPP2B2/2K5 b - - 0 1",
                "3r1k2/4npp1/1ppr3p/p6P/P2PPPP1/1NR5/5K2/2R5 w - - 0 1",
                "2q1rr1k/3bbnnp/p2p1pp1/2pPp3/PpP1P1P1/1P2BNNP/2BQ1PRK/7R b - - 0 1",
                "rnbqkb1r/p3pppp/1p6/2ppP3/3N4/2P5/PPP1QPPP/R1B1KB1R w KQkq - 0 1",
                "r1b2rk1/2q1b1pp/p2ppn2/1p6/3QP3/1BN1B3/PPP3PP/R4RK1 w - - 0 1",
                "2r3k1/pppR1pp1/4p3/4P1P1/5P2/1P4K1/P1P5/8 w - - 0 1",
                "1nk1r1r1/pp2n1pp/4p3/q2pPp1N/b1pP1P2/B1P2R2/2P1B1PP/R2Q2K1 w - - 0 1",
                "4b3/p3kp2/6p1/3pP2p/2pP1P2/4K1P1/P3N2P/8 w - - 0 1",
                "2kr1bnr/pbpq4/2n1pp2/3p3p/3P1P1B/2N2N1Q/PPP3PP/2KR1B1R w - - 0 1",
                "3rr1k1/pp3pp1/1qn2np1/8/3p4/PP1R1P2/2P1NQPP/R1B3K1 b - - 0 1",
                "2r1nrk1/p2q1ppp/bp1p4/n1pPp3/P1P1P3/2PBB1N1/4QPPP/R4RK1 w - - 0 1",
                "r3r1k1/ppqb1ppp/8/4p1NQ/8/2P5/PP3PPP/R3R1K1 b - - 0 1",
                "r2q1rk1/4bppp/p2p4/2pP4/3pP3/3Q4/PP1B1PPP/R3R1K1 w - - 0 1",
                "rnb2r1k/pp2p2p/2pp2p1/q2P1p2/8/1Pb2NP1/PB2PPBP/R2Q1RK1 w - - 0 1",
                "2r3k1/1p2q1pp/2b1pr2/p1pp4/6Q1/1P1PP1R1/P1PN2PP/5RK1 w - - 0 1",
                "r1bqkb1r/4npp1/p1p4p/1p1pP1B1/8/1B6/PPPN1PPP/R2Q1RK1 w kq - 0 1",
                "r2q1rk1/1ppnbppp/p2p1nb1/3Pp3/2P1P1P1/2N2N1P/PPB1QP2/R1B2RK1 b - - 0 1",
                "r1bq1rk1/pp2ppbp/2np2p1/2n5/P3PP2/N1P2N2/1PB3PP/R1B1QRK1 b - - 0 1",
                "3rr3/2pq2pk/p2p1pnp/8/2QBPP2/1P6/P5PP/4RRK1 b - - 0 1",
                "r4k2/pb2bp1r/1p1qp2p/3pNp2/3P1P2/2N3P1/PPP1Q2P/2KRR3 w - - 0 1",
                "3rn2k/ppb2rpp/2ppqp2/5N2/2P1P3/1P5Q/PB3PPP/3RR1K1 w - - 0 1",
                "2r2rk1/1bqnbpp1/1p1ppn1p/pP6/N1P1P3/P2B1N1P/1B2QPP1/R2R2K1 b - - 0 1",
                "r1bqk2r/pp2bppp/2p5/3pP3/P2Q1P2/2N1B3/1PP3PP/R4RK1 b kq - 0 1",
                "r2qnrnk/p2b2b1/1p1p2pp/2pPpp2/1PP1P3/PRNBB3/3QNPPP/5RK1 w - - 0 1",
            };
            var solutions = new[]{
                "Qd1+","d5","f5","e6","a4","g6","Nf6","f5","f5","Ne5","f4","Bf5","b4",
                "Qd2 Qe1","Qxg7+","Ne4","h5","Nb3","Rxe4","g4","Nh6","Bxe4","f6","f4"
            };

            var failures = 0;

            for (var i = 0; i < positions.Length; i++)
            {
                patzer.Position(positions[i]);

                var bestMove = patzer.FindBestMove(10000);

                if (!bestMove.Notation.Equals(solutions[i]))
                {
                    Log.Error($"Failed: {positions[i]} - {bestMove.Notation} != {solutions[i]}");
                    failures++;
                }
                else
                {
                    Log.Information($"Success: {positions[i]} - {bestMove.Notation} == {solutions[i]}");
                }
            }

            Assert.AreEqual(0, failures);
        }

        [TestMethod]
        public void FindMateInOneTest()
        {
            patzer.Position("k7/8/1QP5/8/8/8/8/7K w - - 0 1");

            var bestMove = patzer.FindBestMove();

            Assert.AreEqual("b6b7", bestMove.Notation);
        }

        [TestMethod]
        public void DoNotMoveIntoMateInOne()
        {
            // The engine was moving into mate in one. 
            // Playing h1g1 is a mate in one for black. Don't do that!

            //position fen 2r5/R3n1p1/4kn2/7p/3P4/8/3NPPPP/4KB1R w K - 1 23
            //go
            //info depth 2 nodes 244 nps 22181 score cp 915 time 12 pv h1g1
            //info depth 4 nodes 19528 nps 58819 score cp 896 time 332 pv a7a6
            //info depth 6 nodes 1674452 nps 468116 score cp 868 time 3577 pv e1d1
            //info string debug aborting @ depth 8
            //bestmove h1g1            

            patzer.Position("2r5/R3n1p1/4kn2/7p/3P4/8/3NPPPP/4KB1R w K - 1 23");

            var bestMove = patzer.FindBestMove();

            Assert.AreNotEqual("h1g1", bestMove.Notation);
        }

        [TestMethod]
        public void DoNotPlayQueenD2Test()
        {
            patzer.Position("rnbqkbnr/ppp2ppp/8/4p3/4N3/5N2/PPPP1PPP/R1BQKB1R b KQkq - 0 4");

            var bestMove = patzer.FindBestMove();

            Assert.AreNotEqual("d8d2", bestMove.Notation);
        }

        [TestMethod]
        public void PushAPawnAndDieTest()
        {
            // e8f8 is a good move; pushing e.g. a B pawn is not 

            patzer.Position("rnbqk1nr/ppp1b2p/6P1/3p3Q/3Np3/2N5/PPPP1PPP/R1B1KB1R b KQkq - 0 7");

            var bestMove = patzer.FindBestMove(1000);

            var validMoves = new[] { "e8f8", "g8f6", "h7g6" };

            Assert.IsTrue(validMoves.Contains(bestMove.Notation));
        }

        [TestMethod]
        public void FindMateInThreeTest()
        {
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

            patzer.Position("rnbqkbnr/ppp2ppp/8/3pp3/4P3/3P1N2/PPP2PPP/RNBQKB1R b KQkq - 0 3");

            var bestMove = patzer.FindBestMove(1000);

            Assert.AreNotEqual("d8g5", bestMove.Notation);
        }

        [TestMethod]
        public void NodeEvaluationTest()
        {
            patzer.Position("7k/8/8/8/8/1p6/B7/7K w - - 0 1");

            var bestMove = patzer.FindBestMove();

            Assert.IsTrue(new[] { "a2b1", "a2b3" }.Contains(bestMove.Notation));
        }

        [TestMethod]
        public void MateIn4WinningTest()
        {
            patzer.Position("7k/PP6/8/4K3/8/8/8/8 w - - 0 1");

            var move = patzer.FindBestMove(5000);

            Assert.IsNotNull(move);

            Assert.IsTrue(traceLines.Exists(i => i.Contains("mate 4")));
        }

        [TestMethod]
        public void FindMateInTenTest()
        {
            patzer.Position("4b3/1p6/8/1p1P4/1p6/7P/1P3K1p/7k w - - 0 1");

            Task<AlgebraicMove> task = Task.Run(() => patzer.FindBestMove(30000));

            var foundMate = false;

            while (!task.IsCompleted)
            {
                foundMate = traceLines.Exists(i => i.Contains("mate 10"));

                if (foundMate)
                {
                    patzer.Stop();
                }

                Thread.Sleep(200);
            }

            Assert.IsNotNull(task.Result);
            if (!foundMate)
            {
                Assert.Inconclusive("Patzer is not yet able to go to depth 20.");
            }
        }

        [TestMethod]
        public void MateIn4LosingTest()
        {
            patzer.Position("7k/PP6/8/4K3/8/8/8/8 b - - 0 1");

            var move = patzer.FindBestMove(5000);

            Assert.IsNotNull(move);

            Assert.IsTrue(traceLines.Exists(i => i.Contains("mate -4")));
        }

        [TestMethod]
        [Ignore("long running: 1,5 minutes on dev machine")]
        public void PerftTest()
        {
            var tests = new (string fen, int depth, ulong nodes)[]
            {
                new(ForsythEdwardsNotation.StartingPosition, 5, 4865609),
                new("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -", 5, 193690690),
                new("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -", 6, 11030083),
                new("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", 5, 15833292),
                new("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8", 5, 89941194),
                new("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", 5, 164075551),
            };

            foreach (var (fen, depth, nodes) in tests)
            {
                var success = false;

                patzer.Position(fen);

                patzer.Perft(depth);

                if (traceLines.Exists(line => line.Contains(nodes.ToString())))
                {
                    success = true;

                    Log.Information($"{fen}: {nodes} nodes @ depth {depth}");
                }

                Assert.IsTrue(success);
            }
        }

        [TestMethod]
        [Ignore("Not finished and will run for a long time anyway.")]
        public void StrategicTestSuiteTest()
        {
            var epdLines = new List<string>();
            foreach (var epdFile in new DirectoryInfo("resources/sts").EnumerateFiles("*.epd"))
            {
                epdLines.AddRange(File.ReadAllLines(epdFile.FullName));
            }

            var epdRegex = new Regex(@"(.+)\sbm\s(.+?);");

            foreach (var epdLine in epdLines)
            {
                var match = epdRegex.Match(epdLine);

                if (match.Success)
                {
                    var fen = match.Groups[1].Value;
                    var epdBestMove = match.Groups[2].Value;

                    patzer.Position(fen);

                    var engineBestMove = patzer.FindBestMove();

                    // TODO: Check suggested best move against engine move.

                    Log.Information($"FEN: {fen}; BM: {epdBestMove}; ENGINE: {engineBestMove.Notation}");
                }
                else
                {
                    Assert.Fail($"Unable to parse: {epdLine}");
                }
            }
        }
    }
}