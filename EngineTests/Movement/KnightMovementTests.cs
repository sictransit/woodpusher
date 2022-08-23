﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Engine.Movement;
using SicTransit.Woodpusher.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Engine.Movement.Tests
{
    [TestClass()]
    public class KnightMovementTests
    {
        [TestMethod()]
        public void GetTargetVectorsTest()
        {
            var d4 = Square.FromAlgebraicNotation("d4");

            var moves = new List<Move>();

            foreach (var vector in KnightMovement.GetTargetVectors(d4))
            {
                moves.AddRange(vector);
            }

            Assert.AreEqual(8, moves.Count);
        }
    }
}