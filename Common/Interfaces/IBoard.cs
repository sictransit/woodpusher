using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Interfaces
{
    public interface IBoard
    {
        PieceColor ActiveColor { get; }

        IBoard PlayMove(Move move);

        string GetHash();

        bool IsPassedPawn(Position position);

        IEnumerable<Move> GetOpeningBookMoves();

        IEnumerable<Move> GetLegalMoves();

        IEnumerable<Move> GetLegalMoves(Position position);

        IEnumerable<Position> GetPositions(PieceColor pieceColor);

        IEnumerable<Position> GetPositions(PieceColor pieceColor, PieceType pieceType);

        IBoard SetPosition(Position position);

        IEnumerable<Position> GetAttackers(ulong square, PieceColor color);

        int Score { get; }

        bool IsChecked { get; }

        Counters Counters { get; }
    }
}
