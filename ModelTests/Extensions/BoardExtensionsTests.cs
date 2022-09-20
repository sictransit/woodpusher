using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Parsing;

namespace SicTransit.Woodpusher.Tests.Extensions
{
    [TestClass()]
    public class BoardExtensionsTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Logging.EnableUnitTestLogging(Serilog.Events.LogEventLevel.Debug);
        }

        [TestMethod()]
        public void PerftStartingPositionTest()
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
        public void PerftStartingPositionC2C3Test()
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

            var move = board.GetValidMoves().SingleOrDefault(m => m.Position.Square.Equals(new Square("c2")) && m.Target.Square.Equals(new Square("c3")));

            Assert.IsTrue(PerftAndCompare(board.Play(move), stockfish, 3));
        }

        [TestMethod()]
        public void PerftStartingPositionC2C3D7D6Test()
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

            var c2c3 = board.GetValidMoves().SingleOrDefault(m => m.Position.Square.Equals(new Square("c2")) && m.Target.Square.Equals(new Square("c3")));
            board = board.Play(c2c3);

            var d7d6 = board.GetValidMoves().SingleOrDefault(m => m.Position.Square.Equals(new Square("d7")) && m.Target.Square.Equals(new Square("d6")));
            board = board.Play(d7d6);

            Assert.IsTrue(PerftAndCompare(board, stockfish, 2));
        }

        [TestMethod()]
        public void PerftStartingPositionC2C3D7D6D1A4Test()
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

            var c2c3 = board.GetValidMoves().SingleOrDefault(m => m.Position.Square.Equals(new Square("c2")) && m.Target.Square.Equals(new Square("c3")));
            board = board.Play(c2c3);

            var d7d6 = board.GetValidMoves().SingleOrDefault(m => m.Position.Square.Equals(new Square("d7")) && m.Target.Square.Equals(new Square("d6")));
            board = board.Play(d7d6);

            var d1a4 = board.GetValidMoves().SingleOrDefault(m => m.Position.Square.Equals(new Square("d1")) && m.Target.Square.Equals(new Square("a4")));
            board = board.Play(d1a4);

            Assert.IsTrue(PerftAndCompare(board, stockfish, 1));
        }



        private static bool PerftAndCompare(Board board, string expected, int depth)
        {
            var expectedMoves = expected.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet();

            bool success = true;

            foreach (var move in board.GetValidMoves())
            {
                var result = $"{move.ToAlgebraicMoveNotation()}: {(depth > 1 ? board.Play(move).Perft(depth) : 1)}";

                if (!expectedMoves.Remove(result))
                {
                    Log.Warning($"not expected: {result}");
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