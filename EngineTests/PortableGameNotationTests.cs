using Microsoft.VisualStudio.TestTools.UnitTesting;
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

namespace SicTransit.Woodpusher.Engine.Tests
{
    [TestClass()]
    public class PortableGameNotationTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Logging.EnableUnitTestLogging(Serilog.Events.LogEventLevel.Debug);
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

            IEngine engine = new Patzer();

            foreach (var pgnMove in pgn.PgnMoves)
            {
                var move = pgnMove.GetMove(engine);

                Log.Information($"{pgnMove} -> {move}");

                engine.Play(move);
            }

            Assert.AreEqual(Result.Draw, pgn.Result);
        }

        [TestMethod]
        public void ParseKasparovTopalovTest()
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

            IEngine engine = new Patzer();

            foreach (var pgnMove in pgn.PgnMoves)
            {
                var move = pgnMove.GetMove(engine);

                Log.Information($"{pgnMove} -> {move}");

                engine.Play(move);
            }

            Assert.AreEqual(Result.WhiteWin, pgn.Result);
        }

        [TestMethod]
        public void CheckCrashTest()
        {
            var game = @"
[Event ""?""]
[Site ""?""]
[Date ""2022.09.17""]
[Round ""?""]
[White ""micke""]
[Black ""Woodpusher""]
[Result ""1-0""]
[ECO ""B00""]
[GameDuration ""00:00:07""]
[GameEndTime ""2022-09-17T11:38:13.163 W. Europe Summer Time""]
[GameStartTime ""2022-09-17T11:38:05.687 W. Europe Summer Time""]
[Opening ""St. George (Baker) defense""]
[PlyCount ""5""]
[Termination ""abandoned""]
[TimeControl ""40/300""]

1. e4 a6 2. Qh5 {1.6s} c5 3. Qxf7+ {2.8s, Black disconnects} 1-0
";

            var pgn = PortableGameNotation.Parse(game);

            IEngine engine = new Patzer();

            foreach (var pgnMove in pgn.PgnMoves)
            {
                var move = pgnMove.GetMove(engine);

                Log.Information($"{pgnMove} -> {move}");

                engine.Play(move);
            }

            Assert.AreEqual(Result.WhiteWin, pgn.Result);

        }

        [TestMethod]
        public void CheckRageQuitTest()
        {
            var game = @"
[Event ""?""]
[Site ""?""]
[Date ""2022.09.17""]
[Round ""?""]
[White ""micke""]
[Black ""Woodpusher""]
[Result ""1-0""]
[ECO ""A40""]
[GameDuration ""00:00:17""]
[GameEndTime ""2022-09-17T14:47:17.168 W. Europe Summer Time""]
[GameStartTime ""2022-09-17T14:46:59.733 W. Europe Summer Time""]
[Opening ""Queen's pawn""]
[PlyCount ""15""]
[Termination ""abandoned""]
[TimeControl ""40/300""]

1. d4 f6 2. c4 {2.4s} a5 3. e4 {2.3s} d5 4. cxd5 {1.1s} Ra6 5. Bxa6 {1.9s} Qd7
6. Bxb7 {1.1s} Qg4 7. Bxc8 {1.1s} Qh4 8. Bd7+ {4.4s, Black disconnects} 1-0

";

            var pgn = PortableGameNotation.Parse(game);

            IEngine engine = new Patzer();

            foreach (var pgnMove in pgn.PgnMoves)
            {
                var move = pgnMove.GetMove(engine);

                Log.Information($"{pgnMove} -> {move}");

                engine.Play(move);
            }

            Assert.AreEqual(Piece.None, engine.Board.ActiveColor);

            var moves = engine.Board.GetLegalMoves().ToArray();

            Assert.AreEqual(1, moves.Count(m => m.Piece.Is(Piece.Knight)));
            Assert.AreEqual(3, moves.Count(m => m.Piece.Is(Piece.King)));
        }

        [TestMethod]
        public void StopMovingIntoCheckTest()
        {
            var game = @"
[Event ""?""]
[Site ""?""]
[Date ""2022.09.17""]
[Round ""?""]
[White ""micke""]
[Black ""Woodpusher""]
[Result ""1-0""]
[ECO ""B00""]
[GameDuration ""00:00:36""]
[GameEndTime ""2022-09-17T16:07:44.426 W. Europe Summer Time""]
[GameStartTime ""2022-09-17T16:07:07.668 W. Europe Summer Time""]
[Opening ""Fred""]
[PlyCount ""25""]
[Termination ""illegal move""]
[TimeControl ""40/300""]

1. e4 f5 2. Bb5 {3.4s} f4 3. Bxd7+ {4.1s} Kxd7 4. f3 {3.8s} g6 5. Nh3 {3.3s} b5
6. Nxf4 {1.2s} h5 7. Nxg6 {2.0s} Na6 8. Nxh8 {1.8s} h4 9. h3 {2.1s} Nb8
10. d4 {5.4s} Bh6 11. Bxh6 {3.0s} e6 12. Bg5 {3.5s} Nc6
13. Bxd8 {1.8s, Black makes an illegal move: d7e7} 1-0



";

            var pgn = PortableGameNotation.Parse(game);

            IEngine engine = new Patzer();

            foreach (var pgnMove in pgn.PgnMoves)
            {
                var move = pgnMove.GetMove(engine);

                Log.Information($"{pgnMove} -> {move}");

                engine.Play(move);
            }

            Assert.AreEqual(Piece.None, engine.Board.ActiveColor);

            var moves = engine.Board.GetLegalMoves().ToArray();

            Assert.IsFalse(moves.Any(m => m.Piece.Is(Piece.King) && m.GetTarget().Equals(new Square("e7"))));
        }

        [TestMethod]
        public void MovingIntoPawnCheckTest()
        {
            var game = @"
[Event ""?""]
[Site ""?""]
[Date ""2022.09.17""]
[Round ""?""]
[White ""micke""]
[Black ""Woodpusher""]
[Result ""1-0""]
[ECO ""A00""]
[GameDuration ""00:01:36""]
[GameEndTime ""2022-09-17T16:37:08.870 W. Europe Summer Time""]
[GameStartTime ""2022-09-17T16:35:32.047 W. Europe Summer Time""]
[Opening ""Van't Kruijs Opening""]
[PlyCount ""71""]
[Termination ""illegal move""]
[TimeControl ""40/300""]

1. e3 g5 2. Qg4 {1.8s} b6 3. Qxg5 {0.96s} f6 4. Qxg8 {2.3s} d5 5. Qxf8+ {2.5s}
Kd7 6. Qxh8 {4.0s} Kc6 7. Qxd8 {1.9s} e6 8. Qe8+ {2.2s} Bd7 9. Qxd7+ {2.7s} Kxd7
10. d4 {5.1s} c5 11. Bd2 {1.5s} h5 12. Bb5+ {3.3s} Kc7 13. Ba5 {4.7s} Kc8
14. Bxb6 {1.9s} axb6 15. Bd7+ {2.4s} Kxd7 16. Nf3 {2.7s} c4 17. O-O {1.9s} Kd6
18. Rd1 {2.1s} Nd7 19. Ng5 {3.2s} Nc5 20. Nxe6 {1.2s} Nd3 21. cxd3 {2.0s} h4
22. h3 {1.6s} Ra4 23. dxc4 {2.8s} Rxa2 24. Rxa2 {1.8s} Kd7 25. cxd5 {1.4s} b5
26. Nc3 {2.3s} Kd6 27. Nxb5+ {1.2s} Kd7 28. Nc5+ {4.8s} Kd8 29. Ne6+ {2.7s} Ke7
30. Nec7 {1.9s} Kd8 31. d6 {1.9s} Kd7 32. d5 {3.4s} Kc8 33. Ne6 {5.0s} f5
34. Ra8+ {4.3s} Kb7 35. Nd8+ {5.1s} Kb6
36. Nd4 {5.1s, Black makes an illegal move: b6c7} 1-0

";
            IEngine engine = new Patzer();

            foreach (var pgnMove in PortableGameNotation.Parse(game).PgnMoves)
            {
                engine.Play(pgnMove.GetMove(engine));
            }

            Trace.WriteLine(engine.Board.PrettyPrint());

            Assert.AreEqual(Piece.None, engine.Board.ActiveColor);

            var moves = engine.Board.GetLegalMoves().ToArray();

            Assert.IsFalse(moves.Any(m => m.Piece.Is(Piece.King) && m.GetTarget().Equals(new Square("c7"))));
        }

        [TestMethod]
        public void NoCastlingIfKnightIsInTheWayTest()
        {
            var game = @"
[Event ""?""]
[Site ""?""]
[Date ""2022.09.17""]
[Round ""?""]
[White ""micke""]
[Black ""Woodpusher""]
[Result ""1-0""]
[ECO ""A00""]
[GameDuration ""00:00:40""]
[GameEndTime ""2022-09-17T17:26:18.602 W. Europe Summer Time""]
[GameStartTime ""2022-09-17T17:25:37.690 W. Europe Summer Time""]
[Opening ""Anti-Borg (Desprez) Opening""]
[PlyCount ""33""]
[Termination ""illegal move""]
[TimeControl ""40/300""]

1. h4 f5 2. g4 {1.2s} Nf6 3. f4 {1.1s} e5 4. e4 {1.1s} Bb4 5. c4 {4.9s} Rf8
6. a4 {1.8s} Rh8 7. b3 {5.6s} b5 8. Bd3 {4.2s} bxa4 9. Nf3 {1.3s} axb3
10. O-O {2.4s} Bb7 11. Qxb3 {4.4s} g5 12. Qxb4 {0.80s} d5 13. exd5 {4.6s} Bc6
14. fxe5 {1.1s} Bb5 15. gxf5 {1.5s} Bxc4 16. hxg5 {1.0s} Qd6
17. exd6 {2.1s, Black makes an illegal move: e8c8} 1-0

";
            IEngine engine = new Patzer();

            foreach (var pgnMove in PortableGameNotation.Parse(game).PgnMoves)
            {
                Trace.WriteLine($"will try to play: {pgnMove}");

                engine.Play(pgnMove.GetMove(engine));

                Trace.WriteLine(engine.Board.PrettyPrint());
            }

            Assert.AreEqual(Piece.None, engine.Board.ActiveColor);

            var moves = engine.Board.GetLegalMoves().ToArray();

            Assert.IsFalse(moves.Any(m => m.Piece.Is(Piece.King) && m.GetTarget().Equals(new Square("c8"))));
        }

        [TestMethod]
        public void PromotionToQueenCrashTest()
        {
            var game = @"
[Event ""?""]
[Site ""?""]
[Date ""2022.09.18""]
[Round ""?""]
[White ""micke""]
[Black ""Woodpusher""]
[Result ""1-0""]
[ECO ""B00""]
[GameDuration ""00:00:07""]
[GameEndTime ""2022-09-18T16:43:30.475 W. Europe Summer Time""]
[GameStartTime ""2022-09-18T16:43:22.803 W. Europe Summer Time""]
[Opening ""Barnes defense""]
[PlyCount ""9""]
[Termination ""abandoned""]
[TimeControl ""40/300""]

1. e4 f6 2. e5 {1.5s} b5 3. exf6 {1.1s} d6 4. fxg7 {0.78s} Kd7
5. gxh8=Q {2.7s, Black disconnects} 1-0
";
            IEngine engine = new Patzer();

            foreach (var pgnMove in PortableGameNotation.Parse(game).PgnMoves)
            {
                Trace.WriteLine($"will try to play: {pgnMove}");

                engine.Play(pgnMove.GetMove(engine));

                Trace.WriteLine(engine.Board.PrettyPrint());
            }

            Assert.AreEqual(Piece.None, engine.Board.ActiveColor);

            var playedMove = engine.FindBestMove();

            Assert.IsNotNull(playedMove);
        }

        [TestMethod]
        public void BlackCastlesThroughOwnQueenTest()
        {
            var game = @"
[Event ""?""]
[Site ""?""]
[Date ""2022.09.18""]
[Round ""?""]
[White ""Woodpusher""]
[Black ""Woodpusher""]
[Result ""1-0""]
[ECO ""B01""]
[GameDuration ""00:00:00""]
[GameEndTime ""2022-09-18T17:19:57.934 W. Europe Summer Time""]
[GameStartTime ""2022-09-18T17:19:57.846 W. Europe Summer Time""]
[Opening ""Scandinavian (center counter) defense""]
[PlyCount ""21""]
[Termination ""illegal move""]
[TimeControl ""40/300""]

1. e4 d5 2. Bb5+ Bd7 3. Qg4 g5 4. Bc6 a6 5. Qxd7+ Nxd7 6. Kd1 g4 7. Ke2 h5
8. Kd1 Ra7 9. g3 Ra8 10. Nf3 Rh6 11. h4 {Black makes an illegal move: e8c8} 1-0
";
            IEngine engine = new Patzer();

            foreach (var pgnMove in PortableGameNotation.Parse(game).PgnMoves)
            {
                Trace.WriteLine($"will try to play: {pgnMove}");

                engine.Play(pgnMove.GetMove(engine));

                Trace.WriteLine(engine.Board.PrettyPrint());
            }

            Assert.AreEqual(Piece.None, engine.Board.ActiveColor);

            var moves = engine.Board.GetLegalMoves().ToArray();

            Assert.IsFalse(moves.Any(m => m.Piece.Is(Piece.King) && m.Flags.HasFlag(SpecialMove.CastleQueen)));
        }

        [TestMethod]
        public void ParseWinAgainstStockfishTest()
        {
            var game = @"
[Event ""?""]
[Site ""?""]
[Date ""2022.11.17""]
[Round ""?""]
[White ""stockfish_15_x64_avx2""]
[Black ""Woodpusher (release)""]
[Result ""0-1""]
[ECO ""B00""]
[GameDuration ""00:02:58""]
[GameEndTime ""2022-11-17T20:29:53.126 W. Europe Standard Time""]
[GameStartTime ""2022-11-17T20:26:54.615 W. Europe Standard Time""]
[Opening ""KP""]
[PlyCount ""92""]
[TimeControl ""40/60+0.6""]
[Variation ""Nimzovich defense""]

1. e4 {+0.50/27 11s} Nc6 {-1.71/4 1.4s} 2. Bb5 {+0.61/23 3.8s}
Nd4 {-1.83/6 1.4s} 3. Be2 {+0.72/19 1.1s} Nf6 {-1.95/6 1.5s}
4. e5 {+0.49/18 0.86s} Nxe2 {-3.36/4 1.5s} 5. Qxe2 {+0.26/27 8.6s}
Ng8 {-4.31/6 1.5s} 6. Nc3 {+0.07/19 1.5s} d6 {-3.25/4 1.5s}
7. Nf3 {+0.46/16 0.96s} Bg4 {-4.38/6 1.5s} 8. Qb5+ {+1.24/17 0.79s}
Qd7 {-2.09/6 1.6s} 9. h3 {+1.77/19 2.6s} Qxb5 {-3.94/4 1.6s}
10. Nxb5 {-0.47/21 3.0s} Bxf3 {-4.20/6 1.6s} 11. Nxc7+ {-0.64/21 3.4s}
Kd8 {-1.97/6 1.6s} 12. Nxa8 {-1.19/21 2.8s} Bxg2 {-2.60/6 1.7s}
13. Rf1 {-1.34/20 3.1s} Bxf1 {-1.97/6 1.7s} 14. e6 {-4.07/18 2.5s}
Bxh3 {+0.35/6 1.7s} 15. d3 {-8.22/21 6.7s} Bxe6 {+4.49/6 1.7s}
16. Be3 {-7.48/18 2.0s} a5 {+4.30/6 1.7s} 17. Rb1 {-7.25/16 2.7s}
Bxa2 {+4.79/6 1.8s} 18. Bb6+ {-5.94/18 2.3s} Kc8 {+5.17/6 1.8s}
19. Rd1 {-5.39/19 3.5s} Nf6 {+5.07/6 1.8s} 20. Ra1 {-7.04/19 2.3s}
Be6 {+5.08/6 1.9s} 21. Ba7 {-6.82/19 4.1s} Ng4 {+5.45/6 1.9s}
22. Rxa5 {-6.14/15 1.8s} Rg8 {+4.81/6 1.9s} 23. c4 {-6.28/14 0.96s}
Ne5 {+5.48/6 2.0s} 24. Nb6+ {-7.27/15 1.6s} Kd8 {+5.82/6 2.0s}
25. Na4 {-7.69/15 0.80s} Nxd3+ {+4.85/6 2.1s} 26. Kf1 {-8.52/15 0.64s}
Bxc4 {+5.70/6 2.1s} 27. Kg1 {-10.08/15 0.61s} Ke8 {+4.91/6 2.2s}
28. Be3 {-9.58/16 0.60s} f6 {+5.83/6 2.2s} 29. Nb6 {-10.48/15 0.60s}
Bb3 {+5.06/6 2.3s} 30. Bd4 {-9.28/16 0.60s} e5 {+6.49/6 2.3s}
31. Be3 {-9.22/17 0.60s} Nxb2 {+6.73/6 2.4s} 32. Bc1 {-7.76/16 0.60s}
Nd3 {+9.02/6 2.5s} 33. Ra8+ {-10.11/16 0.60s} Kf7 {+8.84/6 2.6s}
34. Rxf8+ {-10.81/17 0.60s} Rxf8 {+11.24/6 2.7s} 35. Be3 {-14.35/14 0.60s}
e4 {+13.01/6 2.8s} 36. Bd4 {-14.35/14 0.60s} Rd8 {+13.10/6 2.9s}
37. Nd5 {-17.10/13 0.60s} Bxd5 {+12.92/6 3.1s} 38. Kf1 {-19.83/13 0.60s}
Ra8 {+15.93/6 3.3s} 39. Kg2 {-64.00/20 0.60s} e3+ {+16.01/6 3.7s}
40. Kg1 {-M22/31 0.60s} exf2+ {+14.07/6 4.4s} 41. Kf1 {-M12/55 1.7s}
Ra2 {+20.99/6 1.4s} 42. Bc5 {-M10/85 2.1s} Ra1+ {0.00/8 1.4s}
43. Ke2 {-M8/96 0.51s} f1=Q+ {+19.98/6 1.5s} 44. Kd2 {-M6/245 0.15s}
Qd1+ {+30.75/6 1.3s} 45. Kc3 {-M4/245 0.006s} Nxc5 {+25.84/4 0.022s}
46. Kb4 {-M2/245 0.003s} Qb3# {+30.51/2 0.001s, Black mates} 0-1
";

            IEngine engine = new Patzer();

            foreach (var pgnMove in PortableGameNotation.Parse(game).PgnMoves.ToArray())
            {
                engine.Play(pgnMove.GetMove(engine));

                Trace.WriteLine(Environment.NewLine + engine.Board.PrettyPrint());
            }

            Assert.AreEqual(30, -engine.Board.Score / 100d, 0.5);
        }

        [TestMethod]
        public void AnnotationsTest()
        {
            var game = @"
1. e4?! d5? 2. Bb5+?? Bd7 3. Qg4 g5 4. Bc6 a6 5. Qxd7+ Nxd7 6. Kd1 g4 7. Ke2 h5
8. Kd1 Ra7 9. g3 Ra8 10. Nf3 Rh6 11. h4 {Black makes an illegal move: e8c8} 1-0
";
            var pgn = PortableGameNotation.Parse(game);

            Assert.IsTrue(pgn.PgnMoves.Any(m => m.Annotation == PgnAnnotation.None));
            Assert.IsTrue(pgn.PgnMoves.Any(m => m.Annotation == PgnAnnotation.Inaccuracy));
            Assert.IsTrue(pgn.PgnMoves.Any(m => m.Annotation == PgnAnnotation.Mistake));
            Assert.IsTrue(pgn.PgnMoves.Any(m => m.Annotation == PgnAnnotation.Blunder));
        }
    }
}