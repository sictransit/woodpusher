using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Interfaces;

namespace SicTransit.Woodpusher.Common.Interfaces
{
    public interface IEngine
    {
        IBoard Board { get; }

        bool IsValidMove(Move move);
    }
}
