using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Engine;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Parsing;

namespace SicTransit.Woodpusher.Tests
{
    [TestClass()]
    public class PortableGameNotationTests
    {
        private IEngine engine;

        [TestInitialize]
        public void Initialize()
        {
            Logging.EnableUnitTestLogging(Serilog.Events.LogEventLevel.Information);

            engine = new Patzer();
        }

        [TestMethod]
        public void ParseFisherSpasskyTest()
        {
            var pgnFischerSpassky = @"
[Event ""F/S Return Match""]
[Site ""Belgrade, Serbia JUG""]
[Date ""1992.11.04""]
[Round ""29""]
[White ""Fischer, Robert J.""]
[Black ""Spassky, Boris V.""]
[Result ""1/2-1/2""]

1. e4 e5 2. Nf3 Nc6 3. Bb5 a6 {This opening is called the Ruy Lopez.}
4. Ba4 Nf6 5. O-O Be7 6. Re1 b5 7. Bb3 d6 8. c3 O-O 9. h3 Nb8 10. d4 Nbd7 
11. c4 c6 12. cxb5 axb5 13. Nc3 Bb7 14. Bg5 b4 15. Nb1 h6 16. Bh4 c5 17. dxe5
Nxe4 18. Bxe7 Qxe7 19. exd6 Qf6 20. Nbd2 Nxd6 21. Nc4 Nxc4 22. Bxc4 Nb6
23. Ne5 Rae8 24. Bxf7+ Rxf7 25. Nxf7 Rxe1+ 26. Qxe1 Kxf7 27. Qe3 Qg5 28. Qxg5
hxg5 29. b3 Ke6 30. a3 Kd6 31. axb4 cxb4 32. Ra5 Nd5 33. f3 Bc8 34. Kf2 Bf5
35. Ra7 g6 36. Ra6+ Kc5 37. Ke1 Nf4 38. g3 Nxh3 39. Kd2 Kb5 40. Rd6 Kc5 41. Ra6
Nf2 42. g4 Bd3 43. Re6 1/2-1/2
";

            var pgn = PortableGameNotation.Parse(pgnFischerSpassky);

            Assert.IsNotNull(pgn.Tags);
            Assert.IsTrue(pgn.Tags.Any());

            Assert.IsNotNull(pgn.PgnMoves);
            Assert.IsTrue(pgn.PgnMoves.Any());

            foreach (var pgnMove in pgn.PgnMoves)
            {
                var move = pgnMove.GetMove(engine);

                Log.Information($"{pgnMove} -> {move}");

                engine.Play(move);
            }

            Assert.AreEqual(Result.Draw, pgn.Result);
        }

        [TestMethod]
        public void ParseKasparovTopalov()
        {
            var pgnKasparovTopalov = @"
[Event ""It (cat.17)""]
[Site ""Wijk aan Zee (Netherlands)""]
[Date ""1999.??.??""]
[Round ""?""]
[White ""Garry Kasparov""]
[Black ""Veselin Topalov""]
[Result ""1-0""]

1. e4 d6 2. d4 Nf6 3. Nc3 g6 4. Be3 Bg7 5. Qd2 c6 6. f3 b5 7. Nge2 Nbd7 8. Bh6
Bxh6 9. Qxh6 Bb7 10. a3 e5 11. O-O-O Qe7 12. Kb1 a6 13. Nc1 O-O-O 14. Nb3 exd4
15. Rxd4 c5 16. Rd1 Nb6 17. g3 Kb8 18. Na5 Ba8 19. Bh3 d5 20. Qf4+ Ka7 21. Rhe1
d4 22. Nd5 Nbxd5 23. exd5 Qd6 24. Rxd4 cxd4 25. Re7+ Kb6 26. Qxd4+ Kxa5 27. b4+
Ka4 28. Qc3 Qxd5 29. Ra7 Bb7 30. Rxb7 Qc4 31. Qxf6 Kxa3 32. Qxa6+ Kxb4 33. c3+
Kxc3 34. Qa1+ Kd2 35. Qb2+ Kd1 36. Bf1 Rd2 37. Rd7 Rxd7 38. Bxc4 bxc4 39. Qxh8
Rd3 40. Qa8 c3 41. Qa4+ Ke1 42. f4 f5 43. Kc1 Rd2 44. Qa7 1-0";

            var pgn = PortableGameNotation.Parse(pgnKasparovTopalov);

            Assert.IsNotNull(pgn.Tags);
            Assert.IsTrue(pgn.Tags.Any());

            Assert.IsNotNull(pgn.PgnMoves);
            Assert.IsTrue(pgn.PgnMoves.Any());

            foreach (var pgnMove in pgn.PgnMoves)
            {
                var move = pgnMove.GetMove(engine);

                Log.Information($"{pgnMove} -> {move}");

                engine.Play(move);
            }

            Assert.AreEqual(Result.WhiteWin, pgn.Result);
        }
    }

    
}