﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Tests.Movement
{
    [TestClass()]
    public class QueenMovementTests : MovementTests
    {
        [TestMethod]
        public void GetTargetVectorsTest()
        {
            AssertAmountOfLegalMoves(Piece.Queen, Piece.White, "b2", 23);
        }
    }
}