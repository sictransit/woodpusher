using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
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
            
            Assert.IsFalse(PerftAndCompare(board, stockfish,3).ToList().Any());
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

            Assert.IsFalse(PerftAndCompare(board.Play(move), stockfish, 2).ToList().Any());
        }

        [TestMethod()]
        public void PerftStartingPositionC2C3D7D5Test()
        {
            var stockfish = @"
a2a3: 1
b2b3: 1
d2d3: 1
e2e3: 1
f2f3: 1
g2g3: 1
h2h3: 1
c3c4: 1
a2a4: 1
b2b4: 1
d2d4: 1
e2e4: 1
f2f4: 1
g2g4: 1
h2h4: 1
b1a3: 1
g1f3: 1
g1h3: 1
d1c2: 1
d1b3: 1
d1a4: 1
";

            var board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);

            var c2c3 = board.GetValidMoves().SingleOrDefault(m => m.Position.Square.Equals(new Square("c2")) && m.Target.Square.Equals(new Square("c3")));
            board = board.Play(c2c3);
            
            var d7d5 = board.GetValidMoves().SingleOrDefault(m => m.Position.Square.Equals(new Square("d7")) && m.Target.Square.Equals(new Square("d5")));
            board = board.Play(d7d5);

            Assert.IsFalse(PerftAndCompare(board, stockfish, 1).ToList().Any());
        }


        private static IEnumerable<string> PerftAndCompare(Board board, string expected, int depth)
        {
            foreach (var move in board.GetValidMoves())
            {
                var result = $"{move.ToAlgebraicMoveNotation()}: {board.Play(move).Perft(depth)}";

                if (!expected.Contains(result))
                {
                    Log.Warning($"missing: {result}");

                    yield return result;
                }
            }
        }

        
    }
}