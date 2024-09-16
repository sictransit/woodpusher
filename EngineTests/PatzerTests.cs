﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Common.Extensions;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Parsing;
using SicTransit.Woodpusher.Common.Parsing.Enum;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
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
            Logging.EnableUnitTestLogging(Serilog.Events.LogEventLevel.Information);

            traceLines.Clear();
            patzer = new Patzer(PatzerCallback);            
        }

        [TestMethod]
        public void InitializeTest()
        {
            Assert.IsNotNull(patzer.Board);

            Assert.AreEqual(Piece.White, patzer.Board.ActiveColor);

            Assert.AreEqual(Castlings.WhiteKingside | Castlings.WhiteQueenside | Castlings.BlackKingside | Castlings.BlackQueenside, patzer.Board.Counters.Castlings);
        }


        [TestMethod]
        public void PlayMultipleBestMoveTest()
        {
            var moves = new List<AlgebraicMove>();

            for (var i = 0; i < 10; i++)
            {
                var bestMove = patzer.FindBestMove();

                Assert.IsNotNull(bestMove);

                moves.Add(bestMove.Move);

                patzer.Position(ForsythEdwardsNotation.StartingPosition, moves);
            }
        }

        [TestMethod]
        public void TakeTheQueenTest()
        {
            patzer.Position("Q2K3k/8/2p5/3b4/1p6/1nP5/qq6/5r2 b - - 0 93");

            var bestMove = patzer.FindBestMove();

            Assert.AreEqual("a2a8", bestMove.Move.Notation);
        }

        [TestMethod]
        public void FindMateInOneTest()
        {
            patzer.Position("k7/8/1QP5/8/8/8/8/7K w - - 0 1");

            var bestMove = patzer.FindBestMove();

            Assert.AreEqual("b6b7", bestMove.Move.Notation);
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

            Assert.AreNotEqual("h1g1", bestMove.Move.Notation);
        }

        [TestMethod]
        public void DoNotPlayQueenD2Test()
        {
            patzer.Position("rnbqkbnr/ppp2ppp/8/4p3/4N3/5N2/PPPP1PPP/R1BQKB1R b KQkq - 0 4");

            var bestMove = patzer.FindBestMove();

            Assert.AreNotEqual("d8d2", bestMove.Move.Notation);
        }

        [TestMethod]
        public void PushAPawnAndDieTest()
        {
            // e8f8 is a good move; pushing e.g. a B pawn is not 

            patzer.Position("rnbqk1nr/ppp1b2p/6P1/3p3Q/3Np3/2N5/PPPP1PPP/R1B1KB1R b KQkq - 0 7");

            var bestMove = patzer.FindBestMove(1000);

            var validMoves = new[] { "e8f8", "g8f6", "h7g6" };

            Assert.IsTrue(validMoves.Contains(bestMove.Move.Notation));
        }

        [TestMethod]
        public void FindMateInThreeTest()
        {
            patzer.Position("8/pk6/3B4/1B6/8/P1N5/1Pb2KP1/4R3 w - - 1 38");

            var bestMove = patzer.FindBestMove(10000);

            Assert.AreEqual("e1e7", bestMove.Move.Notation);
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

            Assert.AreNotEqual("d8g5", bestMove.Move.Notation);
        }

        [TestMethod]
        public void PromotionIsNotTheBestMoveAfterAll()
        {
            // The engine was making no-op moves instead of promoting.
            // ... and it turns out that Stockfish agrees.

            patzer.Position("6K1/8/1k6/8/6b1/8/6p1/8 b - - 3 156");

            var bestMove = patzer.FindBestMove(10000);

            Assert.IsTrue(bestMove.Move.Notation.Length == 4);
        }

        [TestMethod]
        public void NodeEvaluationTest()
        {
            patzer.Position("7k/8/8/8/8/1p6/B7/7K w - - 0 1");

            var bestMove = patzer.FindBestMove();

            Assert.AreEqual("a2b3", bestMove.Move.Notation);
        }

        [TestMethod]
        public void MateInMovesWinningTest()
        {
            patzer.Position("7k/PP6/8/4K3/8/8/8/8 w - - 0 1");

            var move = patzer.FindBestMove(5000);

            Assert.IsNotNull(move);

            Assert.IsTrue(traceLines.Any(i => i.Contains("mate 4")));
        }

        [TestMethod]
        public void FindMateInTenTest()
        {
            patzer.Position("4b3/1p6/8/1p1P4/1p6/7P/1P3K1p/7k w - - 0 1");
            
            var move = patzer.FindBestMove(30000);

            Assert.IsNotNull(move);

            Assert.IsTrue(traceLines.Any(i => i.Contains("mate 10")));
        }

        [TestMethod]
        public void MateInMovesLosingTest()
        {
            patzer.Position("7k/PP6/8/4K3/8/8/8/8 b - - 0 1");

            var move = patzer.FindBestMove(10000);

            Assert.IsNotNull(move);

            Assert.IsTrue(traceLines.Any(i => i.Contains("mate -4")));
        }

        [TestMethod]
        public void DoNotBlunderSoMuch()
        {
            // Lichess (i.e. Stockfish) vs Woodpusher. Both sides made a lot of mistakes, for different reasons. 
            // Woodpusher had ten seconds per move to think; Stockfish spent less. 
            // The goal of this test is to have a newer version of our engine rethink the erroneous moves and come up with something better.

            var pgnFile = File.ReadAllText("resources/lichess_pgn_2022.11.01_lichess_AI_level_4_vs_woodpusher.KlijXZP3.pgn");

            var pgn = PortableGameNotation.Parse(pgnFile);

            foreach (var pgnMove in pgn.PgnMoves)
            {
                var matchMove = pgnMove.GetMove(patzer);

                if (pgnMove.Annotation != PgnAnnotation.None)
                {
                    Log.Information($"Finding alternative to: {pgnMove}");

                    var foundAlterantive = false;

                    foreach (var thinkingTime in new[] { 100, 1000, 10000 })
                    {
                        var engineMove = patzer.FindBestMove(thinkingTime);

                        Log.Information($"Found (@{thinkingTime} ms): {engineMove}");

                        if (!matchMove.ToAlgebraicMoveNotation().Equals(engineMove.Move.Notation))
                        {
                            foundAlterantive = true;
                            break;
                        }
                    }

                    if (!foundAlterantive)
                    {
                        Log.Information($"\n{patzer.Board.PrettyPrint()}");
                    }
                    Assert.IsTrue(foundAlterantive);                    
                }

                Log.Information($"Playing: {matchMove}");
                patzer.Play(matchMove);
            }
        }

        [TestMethod]
        public void DoNotStopPlaying()
        {
            // Woodpusher vs. Woordpusher in a Cute Chess game. 
            // Suddenly white stopped playing, without any exceptions caught or logged.

            var pgnFile = File.ReadAllText("resources/woodpusher-vs-woodpusher-freeze.pgn");

            var pgn = PortableGameNotation.Parse(pgnFile);

            foreach (var pgnMove in pgn.PgnMoves)
            {
                var matchMove = pgnMove.GetMove(patzer);

                patzer.Play(matchMove);
            }

            var bestMove = patzer.FindBestMove(3000);

            Assert.IsNotNull(bestMove);

            Log.Information($"Best move found: {bestMove}");
        }

        [TestMethod]
        [Ignore("long running: 1,3 minutes on dev machine")]
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

                if (traceLines.Any(l=>l.Contains(nodes.ToString())))
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

                    Log.Information($"FEN: {fen}; BM: {epdBestMove}; ENGINE: {engineBestMove.Move.Notation}");
                }
                else
                {
                    Assert.Fail($"Unable to parse: {epdLine}");
                }
            }
        }
    }
}