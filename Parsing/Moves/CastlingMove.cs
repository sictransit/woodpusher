using Serilog;
using SicTransit.Woodpusher.Common.Interfaces;

namespace SicTransit.Woodpusher.Parsing.Moves
{
    public abstract class CastlingMove : PgnMove
    {
        protected override void Apply(IEngine engine)
        {
            Log.Debug($"applying {this}");
        }
    }
}
