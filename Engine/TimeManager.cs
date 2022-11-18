using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Engine
{
    internal class TimeManager
    {

        // y = 0.577e3.2472x

        private int timeLimit;
        private readonly Stopwatch stopwatch = new();
        private readonly Dictionary<int, List<int>> searches = new();

        public TimeManager()
        {
            
        }

        public void Reset(int timeLimit)
        {
            this.timeLimit = timeLimit;
            stopwatch.Restart();
        }

        public void AddNode(Node node)
        {

        }

    }
}
