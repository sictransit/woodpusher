using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Lookup;
using SicTransit.Woodpusher.Common.Parsing;
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

        [TestMethod]
        public void ParseECO()
        {
            IEngine engine = new Patzer();
            var openingBook = new OpeningBook();

            using var httpClient = new HttpClient();
            foreach (var file in new[] { "a", "b", "c", "d", "e" })
            {
                var url = $"https://raw.githubusercontent.com/lichess-org/chess-openings/master/dist/{file}.tsv";

                using var textStream = httpClient.GetStreamAsync(url).Result;

                using var reader = new StreamReader(textStream);

                string? line;
                while ((line = reader.ReadLine()) != null)
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

                            engine.Play(move);

                            openingBook.AddMove(hash, move);
                        }
                    }
                }
            }

            openingBook.SaveToFile("test.json");
            openingBook.LoadFromFile("test.json");

            var moves = openingBook.GetMoves("AE220C88323102782F94B87DDAD71A60");

            Assert.AreEqual(2, moves.Count());
            Assert.IsTrue(moves.Any(m => m.Notation.Equals("e7e5")));
            Assert.IsTrue(moves.Any(m => m.Notation.Equals("d7d5")));
        }
    }
}