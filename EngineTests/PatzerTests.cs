using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Engine.Tests
{
    [TestClass()]
    public class PatzerTests
    {
        private Patzer? patzer;

        [TestInitialize]
        public void Initialize()
        {
            patzer = new Patzer();
        }

        [TestMethod()]
        public void InitializeTest()
        {
            patzer!.Initialize(FEN.StartingPosition);

            Assert.IsNotNull(patzer.Board);
            Assert.AreEqual(Piece.White, patzer.ActiveColour);
            Assert.AreEqual(Castlings.WhiteKingside | Castlings.WhiteQueenside | Castlings.BlackKingside | Castlings.BlackQueenside, patzer.Castlings);
        }

        [TestMethod()]
        public void PlyFromStartingPositionTest()
        {
            patzer!.Initialize(FEN.StartingPosition);

            var ply = patzer.GetValidPly().ToArray();

            Assert.AreEqual(20, ply.Length);
            Assert.AreEqual(16, ply.Where(p => p.Position.Piece.HasFlag(Piece.Pawn)).Count());
            Assert.AreEqual(4, ply.Where(p => p.Position.Piece.HasFlag(Piece.Knight)).Count());
        }

        [TestMethod()]
        public void PlyFromSicilianOpeningTest()
        {
            var fen = @"rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 1";

            patzer!.Initialize(fen);

            var ply = patzer.GetValidPly().ToArray();

            Assert.AreEqual(15, ply.Where(p => p.Position.Piece.HasFlag(Piece.Pawn)).Count());
            Assert.AreEqual(1, ply.Where(p => p.Position.Piece.HasFlag(Piece.King)).Count());
            Assert.AreEqual(5, ply.Where(p => p.Position.Piece.HasFlag(Piece.Knight)).Count());
            Assert.AreEqual(5, ply.Where(p => p.Position.Piece.HasFlag(Piece.Bishop)).Count());
            Assert.AreEqual(4, ply.Where(p => p.Position.Piece.HasFlag(Piece.Queen)).Count());
        }

        [TestMethod]
        public void WhiteCastlingKingsideTest()
        {
            var fen = @"r1bqk1nr/pppp1ppp/2n5/2b1p3/2B1P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4";

            patzer!.Initialize(fen);

            var ply = patzer.GetValidPly().ToArray();

            Assert.AreEqual(11, ply.Where(p => p.Position.Piece.HasFlag(Piece.Pawn)).Count());
            Assert.AreEqual(1, ply.Where(p => p.Position.Piece.HasFlag(Piece.Queen)).Count());
            Assert.AreEqual(7, ply.Where(p => p.Position.Piece.HasFlag(Piece.Knight)).Count());
            Assert.AreEqual(9, ply.Where(p => p.Position.Piece.HasFlag(Piece.Bishop)).Count());
            Assert.AreEqual(2, ply.Where(p => p.Position.Piece.HasFlag(Piece.Rook)).Count());
            Assert.AreEqual(3, ply.Where(p => p.Position.Piece.HasFlag(Piece.King)).Count());
            Assert.AreEqual(1, ply.Where(p => p.Position.Piece.HasFlag(Piece.King) && p.Move.Flags.HasFlag(MovementFlags.CastleKing)).Count());
        }

        [TestMethod]
        public void BlackCannotCastleTest()
        {
            var fen = @"r1bqk2r/pppp1Npp/2n2n2/2b4Q/2B1P3/8/PPPP1PPP/RNB2RK1 b - - 0 7";

            patzer!.Initialize(fen);

            var ply = patzer.GetValidPly().ToArray();

            Assert.AreEqual(Piece.Black, patzer.ActiveColour);

            Assert.IsTrue(!ply.Any(p => p.Position.Piece.HasFlag(Piece.King) && p.Move.Flags.HasFlag(MovementFlags.CastleKing)));
        }
    }
}