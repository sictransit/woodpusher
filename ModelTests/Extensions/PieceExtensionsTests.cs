using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model.Tests.Extensions
{
    [TestClass]
    public class PieceExtensionsTests
    {
        [TestMethod]
        public void ToCharTest()
        {
            Assert.AreEqual('P', Piece.Pawn.ToChar());
            Assert.AreEqual('R', Piece.Rook.ToChar());
            Assert.AreEqual('N', Piece.Knight.ToChar());
            Assert.AreEqual('B', Piece.Bishop.ToChar());
            Assert.AreEqual('Q', Piece.Queen.ToChar());
            Assert.AreEqual('K', Piece.King.ToChar());
        }


        [TestMethod]
        public void SetGetMaskTest()
        {
            var mask = 1ul;

            var piece = (Piece.White | Piece.Queen).SetMask(mask);

            Assert.IsTrue(piece.Is(Piece.White) && piece.Is(Piece.Queen));

            Assert.AreEqual(mask, piece.GetMask());
        }

        [TestMethod]
        public void SetGetPieceTest()
        {
            var mask = 1ul;

            var piece = Piece.None.SetMask(mask).SetPiece(Piece.Queen | Piece.White);

            Assert.AreEqual(Piece.White | Piece.Queen, piece.GetPiece());
        }

        [TestMethod()]
        public void ToSquareTest()
        {
            var e4 = new Square("e4");
            var piece = Piece.None.SetMask(e4.ToMask());

            Assert.AreEqual(e4, piece.GetSquare());
        }

    }
}