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

            Assert.AreEqual(20, ply.Count());
            Assert.AreEqual(16, ply.Where(p => p.Position.Piece.HasFlag(Piece.Pawn)).Count());
            Assert.AreEqual(4, ply.Where(p => p.Position.Piece.HasFlag(Piece.Knight)).Count());
        }
    }
}