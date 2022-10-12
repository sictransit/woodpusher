﻿using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Interfaces
{
    public interface IBoard
    {
        Pieces ActiveColor { get; }

        IBoard PlayMove(Move move);

        string GetHash();

        bool IsPassedPawn(Pieces position);

        IEnumerable<Move> GetOpeningBookMoves();

        IEnumerable<Move> GetLegalMoves();

        IEnumerable<Move> GetLegalMoves(Pieces position);

        IEnumerable<Pieces> GetPositions(Pieces pieceColor);

        IEnumerable<Pieces> GetPositions(Pieces pieceColor, Pieces pieceType);

        IBoard SetPosition(Pieces position);

        IEnumerable<Pieces> GetAttackers(ulong square, Pieces color);

        int Score { get; }

        bool IsChecked { get; }

        Counters Counters { get; }
    }
}
