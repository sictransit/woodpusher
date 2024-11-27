using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Common.Interfaces
{
    public interface IEngine
    {
        IBoard Board { get; }

        void Initialize(EngineOptions options);

        void Stop();

        void Play(Move move);

        void Position(string fen, IEnumerable<AlgebraicMove>? algebraicMoves = null);

        AlgebraicMove? FindBestMove(int timeLimit = 1000);

        void Perft(int depth);
    }
}
