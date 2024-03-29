﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Tests.Movement
{
    [TestClass()]
    public class PawnMovementTests : MovementTests
    {
        [TestMethod]
        public void GetTargetVectorsWhiteStartingPositionTest()
        {
            AssertAmountOfLegalMoves(Piece.Pawn, Piece.White, "a2", 3);
        }

        [TestMethod]
        public void GetTargetVectorsBlackStartingPositionTest()
        {
            AssertAmountOfLegalMoves(Piece.Pawn, Piece.None, "a7", 3);
        }

        [TestMethod]
        public void GetTargetVectorsWhitePromotePositionTest()
        {
            AssertAmountOfLegalMoves(Piece.Pawn, Piece.White, "b7", 12);
        }

        [TestMethod]
        public void GetTargetVectorsBlackPromotePositionTest()
        {
            AssertAmountOfLegalMoves(Piece.Pawn, Piece.None, "b2", 12);
        }


        [TestMethod]
        public void GetTargetVectorsWhiteEnPassantTest()
        {
            AssertAmountOfLegalMoves(Piece.Pawn, Piece.White, "c5", 5);
        }

        [TestMethod]
        public void GetTargetVectorsBlackEnPassantTest()
        {
            AssertAmountOfLegalMoves(Piece.Pawn, Piece.None, "d4", 5);
        }
    }
}