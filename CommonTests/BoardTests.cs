using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using SicTransit.Woodpusher.Common.Extensions;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Parsing;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Diagnostics;
using System.Text.Json.Nodes;

namespace SicTransit.Woodpusher.Common.Tests
{
    [TestClass()]
    public class BoardTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Logging.EnableUnitTestLogging(Serilog.Events.LogEventLevel.Debug);
        }

        [TestMethod]
        public void BoardTest()
        {
            IBoard board = new Board();

            var whiteKing = Piece.King | Piece.White;
            var blackKing = Piece.King | Piece.None;

            var e1 = new Square("e1");

            board = board.SetPiece(whiteKing.SetSquare(e1));

            var e8 = new Square("e8");

            board = board.SetPiece(blackKing.SetSquare(e8));

            Assert.AreEqual(e1, board.GetPieces(Piece.White).Single(p => p.Is(Piece.King)).GetSquare());
            Assert.AreEqual(e8, board.GetPieces(Piece.None).Single(p => p.Is(Piece.King)).GetSquare());
        }

        [TestMethod]
        public void FillTest()
        {
            IBoard board = new Board();

            var whitePawn = Piece.Pawn | Piece.White;

            for (var f = 0; f < 8; f++)
            {
                for (var r = 0; r < 8; r++)
                {
                    var square = new Square(f, r);

                    board = board.SetPiece(whitePawn.SetSquare(square));
                }
            }

            Assert.AreEqual(64, board.GetPieces(Piece.White).Count(p => p.Is(Piece.Pawn)));
        }

        [TestMethod]
        public void GetPiecesTest()
        {
            IBoard board = new Board();

            var e1 = new Square("e1");
            var whiteKing = (Piece.King | Piece.White).SetSquare(e1);

            var e8 = new Square("e8");
            var blackKing = (Piece.King | Piece.None).SetSquare(e8);


            board = board.SetPiece(whiteKing.SetSquare(e1)).SetPiece(blackKing.SetSquare(e8));

            var whitePieces = board.GetPieces(Piece.White).ToArray();

            Assert.AreEqual(whiteKing, whitePieces.Single());
            Assert.AreEqual(e1, whitePieces.Single().GetSquare());

            var blackPieces = board.GetPieces(Piece.None).ToArray();

            Assert.AreEqual(blackKing, blackPieces.Single());
            Assert.AreEqual(e8, blackPieces.Single().GetSquare());
        }

        [TestMethod]
        public void GetPiecesOnFileByTypeTest()
        {
            IBoard board = new Board();

            var blackPawn1 = Piece.Pawn | Piece.None;
            var e2 = new Square("e2");

            var blackPawn2 = Piece.Pawn | Piece.None;
            var e3 = new Square("e3");

            var blackPawn3 = Piece.Pawn | Piece.None;
            var e4 = new Square("e4");

            board = board.SetPiece(blackPawn1.SetSquare(e2)).SetPiece(blackPawn2.SetSquare(e3)).SetPiece(blackPawn3.SetSquare(e4));

            var pieces = board.GetPieces(Piece.None, Piece.Pawn).Where(p => p.GetSquare().File == 4).ToArray();

            Assert.AreEqual(3, pieces.Length);

            Assert.IsTrue(pieces.All(p => p.Is(Piece.Pawn)));
            Assert.IsTrue(pieces.All(p => new[] { e2, e3, e4 }.Contains(p.GetSquare())));
        }


        [TestMethod]
        public void PlayTest()
        {
            var whiteBishop = Piece.Bishop | Piece.White;
            var blackPawn = Piece.Pawn | Piece.None;

            var c1 = new Square("c1");
            var g7 = new Square("g7");

            var board = new Board().SetPiece(whiteBishop.SetSquare(c1)).SetPiece(blackPawn.SetSquare(g7));

            Assert.AreEqual(0, board.Counters.HalfmoveClock);
            Assert.AreEqual(0, board.Counters.FullmoveNumber);
            Assert.AreEqual(Piece.White, board.Counters.ActiveColor);
            Assert.AreEqual(0ul, board.Counters.EnPassantTarget);

            Trace.WriteLine(board.PrettyPrint());

            var d2 = new Square("d2");

            board = board.PlayMove(new(whiteBishop.SetSquare(c1), d2));
            Assert.AreEqual(Piece.None, board.Counters.ActiveColor);
            Assert.AreEqual(1, board.Counters.HalfmoveClock);
            Assert.AreEqual(0, board.Counters.FullmoveNumber);

            Trace.WriteLine(board.PrettyPrint());

            var g5 = new Square("g5");

            board = board.PlayMove(new(blackPawn.SetSquare(g7), g5.ToMask(), SpecialMove.CannotTake, new Square("g6").ToMask()));
            Assert.AreEqual(Piece.White, board.Counters.ActiveColor);
            Assert.AreEqual(new Square("g6").ToMask(), board.Counters.EnPassantTarget);
            Assert.AreEqual(0, board.Counters.HalfmoveClock);
            Assert.AreEqual(1, board.Counters.FullmoveNumber);

            Trace.WriteLine(board.PrettyPrint());

            board = board.PlayMove(new(whiteBishop.SetSquare(d2), g5));
            Assert.AreEqual(0ul, board.Counters.EnPassantTarget);
            Assert.AreEqual(0, board.Counters.HalfmoveClock);
            Assert.AreEqual(1, board.Counters.FullmoveNumber);

            Trace.WriteLine(board.PrettyPrint());

            Assert.AreEqual(0, board.GetPieces(Piece.None).Count());

            var whitePieces = board.GetPieces(Piece.White).ToArray();
            Assert.AreEqual(1, whitePieces.Length);

            Assert.AreEqual(whiteBishop, whitePieces.Single(p => p.GetSquare().Equals(g5)).GetPiece());

            Trace.WriteLine(board.PrettyPrint());
        }

        [TestMethod()]
        public void GetAttackersTest()
        {
            var board = ForsythEdwardsNotation.Parse("5r2/2Q2n2/5k2/7r/P3P1p1/1B6/5P2/6K1 b - a3 0 34");

            var attackers = board.GetAttackers(new Square("f7").ToMask(), Piece.None).ToArray();

            Assert.AreEqual(2, attackers.Length);
            Assert.IsTrue(attackers.Any(a => a.HasFlag(Piece.Queen | Piece.White)));
            Assert.IsTrue(attackers.Any(a => a.HasFlag(Piece.Bishop | Piece.White)));
        }

        [TestMethod]
        public void MovesFromStartingPieceTest()
        {
            var board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);

            var moves = board.GetLegalMoves().ToArray();

            Assert.AreEqual(20, moves.Length);
            Assert.AreEqual(16, moves.Count(p => p.Piece.Is(Piece.Pawn)));
            Assert.AreEqual(4, moves.Count(p => p.Piece.Is(Piece.Knight)));
        }

        [TestMethod]
        public void MovesFromSicilianOpeningTest()
        {
            var fen = @"rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 1";

            var board = ForsythEdwardsNotation.Parse(fen);

            var moves = board.GetLegalMoves().ToArray();

            Assert.AreEqual(15, moves.Count(p => p.Piece.Is(Piece.Pawn)));
            Assert.AreEqual(1, moves.Count(p => p.Piece.Is(Piece.King)));
            Assert.AreEqual(5, moves.Count(p => p.Piece.Is(Piece.Knight)));
            Assert.AreEqual(5, moves.Count(p => p.Piece.Is(Piece.Bishop)));
            Assert.AreEqual(4, moves.Count(p => p.Piece.Is(Piece.Queen)));
        }

        [TestMethod]
        public void WhiteCastlingKingsideTest()
        {
            var fen = @"r1bqk1nr/pppp1ppp/2n5/2b1p3/2B1P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4";

            var board = ForsythEdwardsNotation.Parse(fen);

            var moves = board.GetLegalMoves().ToArray();

            Assert.AreEqual(11, moves.Count(p => p.Piece.Is(Piece.Pawn)));
            Assert.AreEqual(1, moves.Count(p => p.Piece.Is(Piece.Queen)));
            Assert.AreEqual(7, moves.Count(p => p.Piece.Is(Piece.Knight)));
            Assert.AreEqual(9, moves.Count(p => p.Piece.Is(Piece.Bishop)));
            Assert.AreEqual(2, moves.Count(p => p.Piece.Is(Piece.Rook)));
            Assert.AreEqual(3, moves.Count(p => p.Piece.Is(Piece.King)));
            Assert.AreEqual(1, moves.Count(p => p.Piece.Is(Piece.King) && p.Flags.HasFlag(SpecialMove.CastleKing)));
        }

        [TestMethod]
        public void EnPassantWithTargetTest()
        {
            var fen = @"r1bqkb1r/ppp1p1pp/2np3n/3PPp2/8/8/PPP2PPP/RNBQKBNR w KQkq f6 0 5";

            var board = ForsythEdwardsNotation.Parse(fen);

            Log.Information(Environment.NewLine + board.PrettyPrint());

            var moves = board.GetLegalMoves().ToArray();

            var enPassantMove = moves.SingleOrDefault(p => p.Flags.HasFlag(SpecialMove.EnPassant));

            Assert.IsNotNull(enPassantMove);

            Assert.AreEqual(new Square("f6"), enPassantMove.GetTarget());
        }

        [TestMethod]
        public void BlackCannotCastleTest()
        {
            var fen = @"r1bqk2r/pppp1Npp/2n2n2/2b4Q/2B1P3/8/PPPP1PPP/RNB2RK1 b - - 0 7";

            var board = ForsythEdwardsNotation.Parse(fen);

            var moves = board.GetLegalMoves().ToArray();

            Assert.AreEqual(Piece.None, board.ActiveColor);

            Assert.IsTrue(!moves.Any(m => m.Piece.Is(Piece.King) && m.Flags.HasFlag(SpecialMove.CastleKing)));
        }

        [TestMethod]
        public void BlackCanBlockCheckTest()
        {
            //  +---+---+---+---+---+---+---+---+
            //  | r | n | b | q | k | b | n | r | 8
            //  +---+---+---+---+---+---+---+---+
            //  | p | p | p |   | p | p | p | p | 7
            //  +---+---+---+---+---+---+---+---+
            //  |   |   |   | p |   |   |   |   | 6
            //  +---+---+---+---+---+---+---+---+
            //  |   |   |   |   |   |   |   |   | 5
            //  +---+---+---+---+---+---+---+---+
            //  | Q |   |   |   |   |   |   |   | 4
            //  +---+---+---+---+---+---+---+---+
            //  |   |   | P |   |   |   |   |   | 3
            //  +---+---+---+---+---+---+---+---+
            //  | P | P |   | P | P | P | P | P | 2
            //  +---+---+---+---+---+---+---+---+
            //  | R | N | B |   | K | B | N | R | 1
            //  +---+---+---+---+---+---+---+---+
            //    a   b   c   d   e   f   g   h
            // 
            // Fen: rnbqkbnr/ppp1pppp/3p4/8/Q7/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 2
            // Key: 5E2F301470264169
            // Checkers: a4

            var board = ForsythEdwardsNotation.Parse(@"rnbqkbnr/ppp1pppp/3p4/8/Q7/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 2");

            Log.Information(Environment.NewLine + board.PrettyPrint());

            var moves = board.GetLegalMoves().ToArray();

            Assert.IsTrue(moves.Any(m => m.ToAlgebraicMoveNotation() == "b7b5"));
        }

        [TestMethod]
        public void PeterJonesTestPositionsTest()
        {
            // Credit: https://gist.github.com/peterellisjones (Peter Jones)
            // 16 secs (release build)

            var tests = JsonNode.Parse(File.ReadAllText(Path.Combine("resources", "perft.test.positions.json")));

            var success = true;

            foreach (var test in tests.AsArray())
            {
                var nodes = test["nodes"].GetValue<ulong>();
                var depth = test["depth"].GetValue<int>() + 1;
                var fen = test["fen"].GetValue<string>();

                var board = ForsythEdwardsNotation.Parse(fen);

                var analyzedNodes = board.Perft(depth);

                if (analyzedNodes != nodes)
                {
                    Log.Warning($"{fen} (d:{depth} n:{nodes}) -> {analyzedNodes}");

                    success = false;
                }
            }

            Assert.IsTrue(success);
        }

        [TestMethod()]
        public void TestPositionsDebug1Test()
        {
            // The problem was the castling shouldn't be allowed if the rook has moved.

            var stockfish = @"
h1f1: 18510
h1g1: 23093
h1h2: 53464
h1h3: 56590
h1h4: 56590
h1h5: 54262
h1h6: 42702
h1h7: 8084
h1h8: 32290
e1d1: 40135
e1f1: 35484
e1d2: 71189
e1e2: 71135
e1f2: 71030
e1g1: 26514
";

            var board = ForsythEdwardsNotation.Parse("5k2/8/8/8/8/8/8/4K2R w K - 0 1");

            Assert.IsTrue(PerftAndCompare(board, stockfish, 6));
        }

        [TestMethod()]
        public void TestPositionsDebug2Test()
        {
            // Check detection for castling didn't work at all, i.e. empty squares were ignored.
            // Additionally pieces would run though other pieces to defend their king.

            var stockfish = @"
b7a8: 1
b7c8: 1
e8d7: 1
e8e7: 1
e8f7: 1
";

            var board = ForsythEdwardsNotation.Parse("r3k2r/1b4bq/8/8/8/8/7B/R3K2R w KQkq - 0 1");

            var h2g3 = board.GetLegalMoves().Single(m => m.Piece.GetSquare().Equals(new Square("h2")) && m.GetTarget().Equals(new Square("g3")));
            board = board.PlayMove(h2g3);
            var h7h2 = board.GetLegalMoves().Single(m => m.Piece.GetSquare().Equals(new Square("h7")) && m.GetTarget().Equals(new Square("h2")));
            board = board.PlayMove(h7h2);
            var a1a8 = board.GetLegalMoves().Single(m => m.Piece.GetSquare().Equals(new Square("a1")) && m.GetTarget().Equals(new Square("a8")));
            board = board.PlayMove(a1a8);
            Log.Information(Environment.NewLine + board.PrettyPrint());

            Assert.IsTrue(PerftAndCompare(board, stockfish, 1));
        }

        [TestMethod()]
        [Ignore("long running: did not finish in 40 minutes")]
        public void Perft7StartingPositionTest()
        {
            var stockfish = @"
a2a3: 106743106
b2b3: 133233975
c2c3: 144074944
d2d3: 227598692
e2e3: 306138410
f2f3: 102021008
g2g3: 135987651
h2h3: 106678423
a2a4: 137077337
b2b4: 134087476
c2c4: 157756443
d2d4: 269605599
e2e4: 309478263
f2f4: 119614841
g2g4: 130293018
h2h4: 138495290
b1a3: 120142144
b1c3: 148527161
g1f3: 147678554
g1h3: 120669525
";

            var board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);

            Assert.IsTrue(PerftAndCompare(board, stockfish, 7));
        }

        [TestMethod()]
        [Ignore("long running: 54 sec on release/laptop")]
        public void Perft6StartingPositionTest()
        {
            var stockfish = @"
a2a3: 4463267
b2b3: 5310358
c2c3: 5417640
d2d3: 8073082
e2e3: 9726018
f2f3: 4404141
g2g3: 5346260
h2h3: 4463070
a2a4: 5363555
b2b4: 5293555
c2c4: 5866666
d2d4: 8879566
e2e4: 9771632
f2f4: 4890429
g2g4: 5239875
h2h4: 5385554
b1a3: 4856835
b1c3: 5708064
g1f3: 5723523
g1h3: 4877234
";

            var board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);

            Assert.IsTrue(PerftAndCompare(board, stockfish, 6));
        }

        [TestMethod()]
        public void Perft5StartingPositionTest()
        {
            var stockfish = @"
a2a3: 181046
b2b3: 215255
c2c3: 222861
d2d3: 328511
e2e3: 402988
f2f3: 178889
g2g3: 217210
h2h3: 181044
a2a4: 217832
b2b4: 216145
c2c4: 240082
d2d4: 361790
e2e4: 405385
f2f4: 198473
g2g4: 214048
h2h4: 218829
b1a3: 198572
b1c3: 234656
g1f3: 233491
g1h3: 198502
";

            var board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);

            Assert.IsTrue(PerftAndCompare(board, stockfish, 5));
        }

        [TestMethod()]
        public void Perft4StartingPositionTest()
        {
            var stockfish = @"
a2a3: 8457
b2b3: 9345
c2c3: 9272
d2d3: 11959
e2e3: 13134
f2f3: 8457
g2g3: 9345
h2h3: 8457
a2a4: 9329
b2b4: 9332
c2c4: 9744
d2d4: 12435
e2e4: 13160
f2f4: 8929
g2g4: 9328
h2h4: 9329
b1a3: 8885
b1c3: 9755
g1f3: 9748
g1h3: 8881
";

            var board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);

            Assert.IsTrue(PerftAndCompare(board, stockfish, 4));
        }

        [TestMethod()]
        public void Perft3StartingPositionC2C3Test()
        {
            var stockfish = @"
a7a6: 397
b7b6: 439
c7c6: 441
d7d6: 545
e7e6: 627
f7f6: 396
g7g6: 439
h7h6: 397
a7a5: 438
b7b5: 443
c7c5: 461
d7d5: 566
e7e5: 628
f7f5: 418
g7g5: 440
h7h5: 439
b8a6: 418
b8c6: 462
g8f6: 460
g8h6: 418
";

            var board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);

            var move = board.GetLegalMoves().Single(m => m.Piece.GetSquare().Equals(new Square("c2")) && m.GetTarget().Equals(new Square("c3")));

            Assert.IsTrue(PerftAndCompare(board.PlayMove(move), stockfish, 3));
        }

        [TestMethod()]
        public void Perft2StartingPositionC2C3D7D6Test()
        {
            var stockfish = @"
a2a3: 27
b2b3: 27
d2d3: 27
e2e3: 27
f2f3: 27
g2g3: 27
h2h3: 27
c3c4: 27
a2a4: 27
b2b4: 27
d2d4: 27
e2e4: 27
f2f4: 27
g2g4: 26
h2h4: 27
b1a3: 27
g1f3: 27
g1h3: 27
d1c2: 27
d1b3: 27
d1a4: 6
";

            var board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);

            var c2c3 = board.GetLegalMoves().Single(m => m.Piece.GetSquare().Equals(new Square("c2")) && m.GetTarget().Equals(new Square("c3")));
            board = board.PlayMove(c2c3);

            var d7d6 = board.GetLegalMoves().Single(m => m.Piece.GetSquare().Equals(new Square("d7")) && m.GetTarget().Equals(new Square("d6")));
            board = board.PlayMove(d7d6);

            Assert.IsTrue(PerftAndCompare(board, stockfish, 2));
        }

        [TestMethod()]
        public void Perft1StartingPositionC2C3D7D6D1A4Test()
        {
            var stockfish = @"
c7c6: 1
b7b5: 1
b8c6: 1
b8d7: 1
c8d7: 1
d8d7: 1
";

            var board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);

            var c2c3 = board.GetLegalMoves().Single(m => m.Piece.GetSquare().Equals(new Square("c2")) && m.GetTarget().Equals(new Square("c3")));
            board = board.PlayMove(c2c3);

            var d7d6 = board.GetLegalMoves().Single(m => m.Piece.GetSquare().Equals(new Square("d7")) && m.GetTarget().Equals(new Square("d6")));
            board = board.PlayMove(d7d6);

            var d1a4 = board.GetLegalMoves().Single(m => m.Piece.GetSquare().Equals(new Square("d1")) && m.GetTarget().Equals(new Square("a4")));
            board = board.PlayMove(d1a4);

            Assert.IsTrue(PerftAndCompare(board, stockfish, 1));
        }

        [TestMethod()]
        public void IsPassedPawnTest()
        {
            var board = ForsythEdwardsNotation.Parse("8/8/7p/1P2Pp1P/2Pp1PP1/8/8/8 w - - 0 1");

            var whitePassedPawns = board.GetPieces(Piece.White, Piece.Pawn).Where(p => board.IsPassedPawn(p)).ToArray();

            Assert.AreEqual(3, whitePassedPawns.Length);

            var blackPassedPawns = board.GetPieces(Piece.None, Piece.Pawn).Where(p => board.IsPassedPawn(p)).ToArray();

            Assert.AreEqual(1, blackPassedPawns.Length);
        }


        private static bool PerftAndCompare(IBoard board, string expected, int depth)
        {
            var expectedMoves = expected.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet();

            var success = true;

            foreach (var move in board.GetLegalMoves())
            {
                var result = $"{move.ToAlgebraicMoveNotation()}: {(depth > 1 ? board.PlayMove(move).Perft(depth) : 1)}";

                if (!expectedMoves.Remove(result))
                {
                    Log.Warning($"not expected: {result}");

                    success = false;
                }
            }

            foreach (var expectedMove in expectedMoves)
            {
                Log.Warning($"expected: {expectedMove}");

                success = false;
            }

            return success;
        }

    }
}