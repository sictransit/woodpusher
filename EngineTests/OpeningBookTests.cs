using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Lookup;
using SicTransit.Woodpusher.Common.Parsing;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Extensions;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher.Engine.Tests
{
    [TestClass()]
    public class OpeningBookTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Logging.EnableUnitTestLogging(Serilog.Events.LogEventLevel.Information);
        }

        //[Ignore("external content")]
        [TestMethod]
        public void ParseECO()
        {
            IEngine engine = new Patzer();
            var openingBook = new OpeningBook(true);

            using var httpClient = new HttpClient();
            foreach (var file in new[] { "a", "b", "c", "d", "e" })
            {
                var url = $"https://raw.githubusercontent.com/lichess-org/chess-openings/master/dist/{file}.tsv";

                using var textStream = httpClient.GetStreamAsync(url).Result;

                using var reader = new StreamReader(textStream);

                while (reader.ReadLine() is { } line)
                {
                    var parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    var eco = parts[0];

                    if (Regex.IsMatch(eco, @"^[ABCDE]\d{2}$"))
                    {
                        var name = parts[1];
                        var pgn = parts[2];
                        var uci = parts[3];
                        var epd = parts[4];

                        var opening = PortableGameNotation.Parse(pgn);

                        Assert.IsTrue(opening.PgnMoves.Any());

                        Log.Information($"{eco} {name} {pgn} {opening.PgnMoves.Count}");

                        engine.Initialize();

                        foreach (var pgnMove in opening.PgnMoves)
                        {
                            var move = pgnMove.GetMove(engine);

                            var hash = engine.Board.GetHash();

                            if (hash == ulong.MaxValue)
                            {
                                Debugger.Break();
                            }

                            engine.Play(move);

                            openingBook.AddMove(hash, move);
                        }
                    }
                }
            }

            openingBook.SaveToFile("test.json");
            openingBook.LoadFromFile("test.json");

            Assert.IsFalse(openingBook.GetMoves(ulong.MinValue).Any());
            Assert.IsFalse(openingBook.GetMoves(ulong.MaxValue).Any());

            var moves = openingBook.GetMoves(11121976597367932187); // starting position           

            Assert.AreEqual(20, moves.Count());
            Assert.IsTrue(moves.Any(m => m.Notation.Equals("g1h3")));
            Assert.IsTrue(moves.Any(m => m.Notation.Equals("d2d4")));
        }

        [TestMethod]
        public void GrünfeldDefenseCounterthrustVariation()
        {
            // E60 Grünfeld Defense: Counterthrust Variation d2d4 g8f6 c2c4 g7g6 g2g3 f8g7 f1g2 d7d5

            IEngine engine = new Patzer();
            var book = new OpeningBook();

            foreach (var notation in "d2d4 g8f6 c2c4 g7g6 g2g3 f8g7 f1g2 d7d5".Split())
            {
                var algebraicMove = AlgebraicMove.Parse(notation);

                var hash = engine.Board.GetHash();

                var suggestedMoves = book.GetMoves(hash);

                Assert.IsTrue(suggestedMoves.Any(m => m.Equals(algebraicMove)));

                var move = engine.Board.GetLegalMoves().SingleOrDefault(m => m.Piece.GetSquare().Equals(algebraicMove.From) && m.GetTarget().Equals(algebraicMove.To) && m.PromotionType == algebraicMove.Promotion);

                Assert.IsNotNull(move);

                engine.Play(move);
            }
        }
    }
}