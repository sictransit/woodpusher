﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Movement;

namespace SicTransit.Woodpusher.Tests.Movement
{
    [TestClass()]
    public class KnightMovementTests
    {
        [TestMethod]
        public void GetTargetVectorsTest()
        {
            var d4 = new Square("d4");

            var targets = new List<Target>();

            foreach (var vector in KnightMovement.GetTargetVectors(d4))
            {
                targets.AddRange(vector);
            }

            Assert.AreEqual(8, targets.Count);
        }
    }
}