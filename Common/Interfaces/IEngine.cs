using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Common.Interfaces
{
    public interface IEngine
    {
        Board Board { get; }

        void Play(Move move);

        void Play(AlgebraicMove algebraicMove);

        AlgebraicMove PlayBestMove();
    }
}
