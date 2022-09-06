using Serilog;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Parsing.Moves
{
    public abstract class CastlingMove : PgnMove
    {
        protected override Move CreateMove(IEngine engine)
        {
            return default;
        }
    }
}
