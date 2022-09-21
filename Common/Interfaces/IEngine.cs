using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Common.Interfaces
{
    public interface IEngine
    {
        Board Board { get; }

        void Initialize();

        void Play(Move move);

        void Position(string fen, IReadOnlyCollection<AlgebraicMove> algebraicMoves);

        AlgebraicMove FindBestMove();
    }
}
