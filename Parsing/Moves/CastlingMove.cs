using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Parsing.Moves
{
    public abstract class CastlingMove : PgnMove
    {
        protected CastlingMove(string raw) : base(raw)
        {
        }

        protected override Move CreateMove(IEngine engine)
        {
            return default;
        }
    }
}
