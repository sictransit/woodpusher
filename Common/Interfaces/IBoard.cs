using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Interfaces
{
    public interface IBoard
    {
        Pieces ActiveColor { get; }

        IBoard PlayMove(Move move);

        string GetHash();

        bool IsPassedPawn(Pieces piece);

        IEnumerable<Move> GetOpeningBookMoves();

        IEnumerable<Move> GetLegalMoves();

        IEnumerable<Move> GetLegalMoves(Pieces piece);

        IEnumerable<Pieces> GetPieces(Pieces pieceColor);

        IEnumerable<Pieces> GetPieces(Pieces pieceColor, Pieces pieceType);

        IBoard SetPiece(Pieces piece);

        IEnumerable<Pieces> GetAttackers(ulong square, Pieces color);

        int Score { get; }

        bool IsChecked { get; }

        Counters Counters { get; }
    }
}
