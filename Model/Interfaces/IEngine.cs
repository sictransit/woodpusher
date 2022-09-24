namespace SicTransit.Woodpusher.Model.Interfaces
{
    public interface IEngine
    {
        IBoard Board { get; }

        void Initialize();

        void Stop();

        void Play(Move move);

        void Position(string fen, IEnumerable<AlgebraicMove> algebraicMoves);

        AlgebraicMove FindBestMove(int timeLimit, Action<string> infoCallback = null);
    }
}
