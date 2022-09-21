using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Common.Interfaces
{
    public interface IEngine
    {
        Board Board { get; }

        void Initialize();

        void Stop();

        void Play(Move move);

        void Position(string fen, IEnumerable<AlgebraicMove> algebraicMoves);

        AlgebraicMove FindBestMove(int timeLimit, Action<string> infoCallback = null);
    }
}
