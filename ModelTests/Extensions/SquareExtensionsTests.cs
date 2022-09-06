﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Tests.Extensions
{
    [TestClass()]
    public class SquareExtensionsTests
    {
        [TestMethod]
        public void ToMaskTest()
        {
            Assert.AreEqual(1ul, new Square("a1").ToMask());
            Assert.AreEqual(1ul << 8, new Square("a2").ToMask());
            Assert.AreEqual(1ul << 9, new Square("b2").ToMask());
            Assert.AreEqual(1ul << 63, new Square("h8").ToMask());
        }

        [TestMethod]
        public void ToSquareTest()
        {
            Assert.AreEqual(new Square("a1"), 1ul.ToSquare());
            Assert.AreEqual(new Square("a2"), (1ul << 8).ToSquare());
            Assert.AreEqual(new Square("b2"), (1ul << 9).ToSquare());
            Assert.AreEqual(new Square("h8"), (1ul << 63).ToSquare());
        }
    }
}