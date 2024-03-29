﻿using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Common.Interfaces
{
    public interface IEngine
    {
        IBoard Board { get; }

        void Initialize();

        void Stop();

        void Play(Move move);

        void Position(string fen, IEnumerable<AlgebraicMove>? algebraicMoves = null);

        BestMove FindBestMove(int timeLimit = 1000);

        void Perft(int depth);
    }
}
