using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Common.Exceptions
{
    public class PatzerException : Exception
    {
        public PatzerException(string? message) : base(message)
        {
        }
    }
}
