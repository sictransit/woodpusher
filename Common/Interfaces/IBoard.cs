using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Interfaces
{
    public interface IBoard
    {
        Piece ActiveColor { get; }

        IBoard PlayMove(Move move);

        bool IsPassedPawn(Piece piece);

        IEnumerable<Move> GetOpeningBookMoves();

        IEnumerable<Move> GetLegalMoves();

        IEnumerable<Move> GetLegalMoves(Piece piece);

        IEnumerable<Piece> GetPieces();

        IEnumerable<Piece> GetPieces(Piece color);

        IEnumerable<Piece> GetPieces(Piece color, Piece type);

        IBoard SetPiece(Piece piece);

        IEnumerable<Piece> GetAttackers(ulong square, Piece color);

        ulong Hash { get; }

        int Score { get; }

        bool IsChecked { get; }

        Counters Counters { get; }
    }
}
