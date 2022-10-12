using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Interfaces
{
    public interface IBoard
    {
        Piece ActiveColor { get; }

        IBoard PlayMove(Move move);

        string GetHash();

        bool IsPassedPawn(Piece piece);

        IEnumerable<Move> GetOpeningBookMoves();

        IEnumerable<Move> GetLegalMoves();

        IEnumerable<Move> GetLegalMoves(Piece piece);

        IEnumerable<Piece> GetPieces(Piece pieceColor);

        IEnumerable<Piece> GetPieces(Piece pieceColor, Piece pieceType);

        IBoard SetPiece(Piece piece);

        IEnumerable<Piece> GetAttackers(ulong square, Piece color);

        int Score { get; }

        bool IsChecked { get; }

        Counters Counters { get; }
    }
}
