using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Common.Parsing;
using SicTransit.Woodpusher.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Engine.Tests
{
    [TestClass()]
    public class OpeningBookTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Logging.EnableUnitTestLogging(Serilog.Events.LogEventLevel.Debug);
        }

        [TestMethod()]
        public void OpeningBookTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void ParseECO()
        {
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

                        var parsed = PortableGameNotation.Parse(pgn);

                        Assert.IsTrue(parsed.PgnMoves.Any());

                        Log.Information($"{eco} {name} {pgn} {parsed.PgnMoves.Count}");
                    }
                }
            }
        }
    }
}