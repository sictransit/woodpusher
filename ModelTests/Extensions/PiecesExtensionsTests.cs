using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model.Tests.Extensions
{
    [TestClass]
    public class PiecesExtensionsTests
    {
        [TestMethod]
        public void SetGetMaskTest()
        {
            var mask = 1ul;

            var piece = (Pieces.White | Pieces.Queen).SetMask(mask);

            Assert.IsTrue(piece.HasFlag(Pieces.White) && piece.HasFlag(Pieces.Queen));

            Assert.AreEqual(mask, piece.GetMask());
        }

        [TestMethod]
        public void SetGetPieceTest()
        {
            var mask = 1ul;

            var piece = Pieces.None.SetMask(mask).SetPiece(Pieces.Queen | Pieces.White);

            Assert.AreEqual(Pieces.White | Pieces.Queen, piece.GetPiece());
        }

        [TestMethod()]
        public void ToSquareTest()
        {
            var e4 = new Square("e4");
            var piece = Pieces.None.SetMask(e4.ToMask());

            Assert.AreEqual(e4, piece.ToSquare());
        }

    }
}