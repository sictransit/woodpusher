using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Common
{
    public class EngineOptions
    {
        public bool UseOpeningBook { get; set; } = false;

        public static EngineOptions Default => new();
    }
}
