using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class TargetExtensions
    {
        public static Move ToMove(this Target target, Position position)
        {
            return new Move(position, target);
        }
    }
}
