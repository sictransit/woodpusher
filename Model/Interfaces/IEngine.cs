﻿namespace SicTransit.Woodpusher.Model.Interfaces
{
    public interface IEngine
    {
        IBoard Board { get; }

        void Initialize();

        void Stop();

        void Play(Move move);

        void Position(string fen, IEnumerable<AlgebraicMove>? algebraicMoves = null);

        AlgebraicMove FindBestMove(int timeLimit = 1000, Action<string>? infoCallback = null);
    }
}
