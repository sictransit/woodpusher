using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Interfaces
{
    public interface IBoard
    {
        Piece ActiveColor { get; }

        IBoard Play(Move move);

        bool IsPassedPawn(Piece piece);

        IEnumerable<LegalMove> GetLegalMoves();

        IEnumerable<LegalMove> GetLegalMoves(Piece piece);

        IEnumerable<Piece> GetPieces();

        IEnumerable<Piece> GetPieces(Piece color);

        IEnumerable<Piece> GetPieces(Piece color, Piece type);

        IBoard SetPiece(Piece piece);

        IEnumerable<Piece> GetAttackers(Piece piece);

        bool IsAttacked(Piece piece);

        Piece FindKing(Piece color);

        ulong Hash { get; }

        int Score { get; }

        bool IsChecked { get; }

        Counters Counters { get; }

        Move LastMove { get; }
    }
}
