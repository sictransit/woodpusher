﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Movement;

namespace SicTransit.Woodpusher.Tests.Movement
{
    [TestClass()]
    public class QueenMovementTests
    {
        [TestMethod]
        public void GetTargetVectorsTest()
        {
            var b2 = new Square("b2");

            var targets = new List<Target>();

            foreach (var vector in QueenMovement.GetTargetVectors(b2))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(23, targets.Count);
        }
    }
}