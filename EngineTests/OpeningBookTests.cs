using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Common.Lookup;
using SicTransit.Woodpusher.Common.Parsing;
using SicTransit.Woodpusher.Common.Parsing.Enum;
using SicTransit.Woodpusher.Common.Parsing.Exceptions;
using System.IO.Compression;
using System.Text;

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
        [Ignore("external content")]
        public void GenerateOpeningBooks()
        {
            static bool eloPredicate(PortableGameNotation game)
            {
                if (!game.WhiteElo.HasValue || !game.BlackElo.HasValue)
                {
                    return false;
                }

                if ((game.WhiteElo.Value + game.BlackElo.Value) / 2 < 2000)
                {
                    return false;
                }

                if (Math.Abs(game.WhiteElo.Value - game.BlackElo.Value) > 500)
                {
                    return false;
                }

                return true;
            }

            var root = new DirectoryInfo(@"C:\Temp\Chess Games");

            var whiteBook = new OpeningBook(Model.Enums.Piece.White, true);
            var blackBook = new OpeningBook(Model.Enums.Piece.None, true);

            foreach (var zipFile in root.EnumerateFiles("*.zip", SearchOption.AllDirectories))
            {
                var games = new List<PortableGameNotation>();

                Log.Information($"Parsing PGN: {zipFile.FullName}");

                using var zipArchive = ZipFile.OpenRead(zipFile.FullName);

                foreach (var zipEntry in zipArchive.Entries)
                {
                    using var reader = new StreamReader(zipEntry.Open(), Encoding.UTF8);

                    var sb = new StringBuilder();

                    var pgns = new List<string>();

                    while (reader.ReadLine() is { } line)
                    {
                        if (line.StartsWith("[Event"))
                        {
                            pgns.Add(sb.ToString());

                            sb.Clear();
                        }

                        sb.AppendLine(line);
                    }

                    foreach (var pgn in pgns)
                    {
                        var game = PortableGameNotation.Parse(pgn);

                        if (eloPredicate(game))
                        {
                            games.Add(game);
                        }
                    }
                }

                Log.Information($"Found games: {games.Count}");

                foreach (var white in new[] { true, false })
                {
                    var openingBook = white ? whiteBook : blackBook;
                    var engine = new Patzer();

                    foreach (var game in games.Where(g => g.PgnMoves.Any() && g.Result == (white ? Result.WhiteWin : Result.BlackWin)).OrderByDescending(g => white ? g.WhiteElo : g.BlackElo).Take(1000))
                    {
                        engine.Initialize();

                        try
                        {
                            foreach (var pgnMove in game.PgnMoves.Take(40))
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

                Log.Information($"Done playing!");
            }

            whiteBook.Prune(10);
            blackBook.Prune(10);

            whiteBook.SaveToFile();
            blackBook.SaveToFile();

            Assert.IsTrue(File.Exists(whiteBook.BookFilename));
            Assert.IsTrue(File.Exists(blackBook.BookFilename));
        }
    }
}