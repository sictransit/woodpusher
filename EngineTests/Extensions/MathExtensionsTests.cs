﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Engine.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Engine.Extensions.Tests
{
    [TestClass()]
    public class MathExtensionsTests
    {
        [TestMethod()]
        public void IdealApproximateNextDepthTimeTest()
        {
            var progress = new List<(int depth,  long time)>();

            var n = 5;

            for (int i = 0; i < n; i++)
            {
                progress.Add(((i+1)*2, (long)(Math.Exp(i)*100)));
            }

            var estimated = MathExtensions.ApproximateNextDepthTime(progress);

            Assert.AreEqual(estimated, (long)(Math.Exp(n) * 100), estimated/100);
        }

        [TestMethod()]
        public void RealApproximateNextDepthTimeTest()
        {
            var progress = new List<(int depth, long time)>
            {
                (4, 1),
                (6, 22),
                (8, 58),
                (10, 414),
                (12, 2159)
            };

            var depth14 = 14336;
 
            var estimated = MathExtensions.ApproximateNextDepthTime(progress);

            Assert.AreEqual(estimated, depth14, depth14 / 10);
        }
    }
}