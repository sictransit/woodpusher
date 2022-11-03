using SicTransit.Woodpusher.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Engine
{
    internal class PrincipalVariationNode
    {
        public PrincipalVariationNode(Move move, int score)
        {
            Move = move;
            Score = score;
        }

        public Move Move { get; set; }
        public int Score { get; set; }
    }
}
