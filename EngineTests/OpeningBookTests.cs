using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Common.Extensions;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Lookup;
using SicTransit.Woodpusher.Common.Parsing;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Extensions;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using SicTransit.Woodpusher.Common.Parsing.Enum;
using SicTransit.Woodpusher.Common.Parsing.Exceptions;

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
        public void GamesTest()
        {
            var root = new DirectoryInfo(@"C:\tmp\Chess Games");

            var engine = new Patzer();
            var openingBook = new OpeningBook(true);

            foreach (var zipFile in root.EnumerateFiles("*.zip", SearchOption.AllDirectories))
            {
                var games = new List<PortableGameNotation>();

                Log.Information($"Parsing PGN: {zipFile.FullName}");

                using var zipArchive = ZipFile.OpenRead(zipFile.FullName);

                foreach (var zipEntry in zipArchive.Entries)
                {
                    using var reader = new StreamReader(zipEntry.Open(), Encoding.UTF8);

                    var sb = new StringBuilder();

                    while (reader.ReadLine() is { } headerLine)
                    {
                        if (headerLine.StartsWith("[Event"))
                        {
                            games.Add(PortableGameNotation.Parse(sb.ToString()));

                            sb.Clear();
                        }

                        sb.AppendLine(headerLine);
                    }

                    games.Add(PortableGameNotation.Parse(sb.ToString()));
                }

                Log.Information($"Total: {games.Count}");

                foreach (var game in games.Where(g=>g.PgnMoves.Any() && g.Result != Result.Ongoing))
                {
                    engine.Initialize();

                    try
                    {
                        foreach (var pgnMove in game.PgnMoves.Take(30))
                        {
                            var move = pgnMove.GetMove(engine);

                            var hash = engine.Board.Hash;

                            engine.Play(move);

                            openingBook.AddMove(hash, move);
                        }
                    }
                    catch (PgnParsingException e)
                    {
                        Log.Error(e, game.Source);
                    }
                }
            }

            openingBook.SaveToFile("games.json");
            openingBook.LoadFromFile("games.json");
        }

        [Ignore("external content")]
        [TestMethod]
        public void ParseECO()
        {
            IEngine engine = new Patzer();
            var openingBook = new OpeningBook(true);

            var maxLength = 0;
            var longest = string.Empty;

            using var httpClient = new HttpClient();
            foreach (var file in new[] { "a", "b", "c", "d", "e" })
            {
                var url = $"https://raw.githubusercontent.com/lichess-org/chess-openings/master/{file}.tsv";

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
                        //var uci = parts[3];
                        //var epd = parts[4];

                        var opening = PortableGameNotation.Parse(pgn);

                        Assert.IsTrue(opening.PgnMoves.Any());

                        Log.Information($"{eco} {name} {pgn} {opening.PgnMoves.Count}");

                        engine.Initialize();

                        if (opening.PgnMoves.Count > maxLength)
                        {
                            longest = line;
                            maxLength = opening.PgnMoves.Count;
                        }

                        foreach (var pgnMove in opening.PgnMoves)
                        {
                            var move = pgnMove.GetMove(engine);

                            var hash = engine.Board.Hash;

                            engine.Play(move);

                            openingBook.AddMove(hash, move);
                        }
                    }
                }
            }

            Log.Information($"Longest opening: {longest}");

            openingBook.SaveToFile("test.json");
            openingBook.LoadFromFile("test.json");

            Assert.IsFalse(openingBook.GetMoves(ulong.MinValue).Any());
            Assert.IsFalse(openingBook.GetMoves(ulong.MaxValue).Any());

            var moves = openingBook.GetMoves(11121976597367932187).ToArray(); // starting position           

            Assert.AreEqual(20, moves.Length);
            Assert.IsTrue(moves.Any(m => m.Notation.Equals("g1h3")));
            Assert.IsTrue(moves.Any(m => m.Notation.Equals("d2d4")));
        }



        [TestMethod]
        public void C89RuyLopezMarshallAttackMainLineSpasskyVariation()
        {
            // C89	Ruy Lopez: Marshall Attack, Main Line, Spassky Variation	
            // 1. e4 e5 2. Nf3 Nc6 3. Bb5 a6 4. Ba4 Nf6 5. O-O Be7 6. Re1 b5 7. Bb3 O-O 8. c3 d5 9. exd5 Nxd5 10. Nxe5 Nxe5 11. Rxe5 c6 12. d4 Bd6 13. Re1 Qh4 14. g3 Qh3 15. Be3 Bg4 16. Qd3 Rae8 17. Nd2 Re6 18. a4 Qh5	
            // e2e4 e7e5 g1f3 b8c6 f1b5 a7a6 b5a4 g8f6 e1g1 f8e7 f1e1 b7b5 a4b3 e8g8 c2c3 d7d5 e4d5 f6d5 f3e5 c6e5 e1e5 c7c6 d2d4 e7d6 e5e1 d8h4 g2g3 h4h3 c1e3 c8g4 d1d3 a8e8 b1d2 e8e6 a2a4 h3h5	
            // 5rk1/5ppp/p1pbr3/1p1n3q/P2P2b1/1BPQB1P1/1P1N1P1P/R3R1K1 w - -

            IEngine engine = new Patzer();
            var book = new OpeningBook();

            foreach (var notation in "e2e4 e7e5 g1f3 b8c6 f1b5 a7a6 b5a4 g8f6 e1g1 f8e7 f1e1 b7b5 a4b3 e8g8 c2c3 d7d5 e4d5 f6d5 f3e5 c6e5 e1e5 c7c6 d2d4 e7d6 e5e1 d8h4 g2g3 h4h3 c1e3 c8g4 d1d3 a8e8 b1d2 e8e6 a2a4 h3h5".Split())
            {
                var algebraicMove = AlgebraicMove.Parse(notation);

                var suggestedMove = book.GetMoves(engine.Board.Hash).SingleOrDefault(m => m.Equals(algebraicMove));

                Assert.IsNotNull(suggestedMove);

                var move = engine.Board.GetLegalMoves().SingleOrDefault(m => m.ToAlgebraicMoveNotation().Equals(algebraicMove.Notation));

                Assert.IsNotNull(move);

                engine.Play(move);
            }

            Log.Information("\n" + engine.Board.PrettyPrint());

            var hash = engine.Board.Hash;

            var fenBoard = ForsythEdwardsNotation.Parse("5rk1/5ppp/p1pbr3/1p1n3q/P2P2b1/1BPQB1P1/1P1N1P1P/R3R1K1 w - -");

            Log.Information("\n" + fenBoard.PrettyPrint());

            Assert.AreEqual(hash, fenBoard.Hash);
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

                var hash = engine.Board.Hash;

                var suggestedMoves = book.GetMoves(hash);

                Assert.IsTrue(suggestedMoves.Any(m => m.Equals(algebraicMove)));

                var move = engine.Board.GetLegalMoves().SingleOrDefault(m => m.Piece.GetSquare().Equals(algebraicMove.From) && m.GetTarget().Equals(algebraicMove.To) && m.PromotionType == algebraicMove.Promotion);

                Assert.IsNotNull(move);

                engine.Play(move);
            }
        }
    }
}