using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Engine;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}