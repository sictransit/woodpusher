using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model.Interfaces
{
    public interface IBoard
    {
        PieceColor ActiveColor { get; }

        IBoard PlayMove(Move move);

        bool IsOccupied(ulong mask, PieceColor color);

        IEnumerable<Move> GetLegalMoves();

        IEnumerable<Move> GetLegalMoves(Position position);

        IEnumerable<Position> GetPositions();

        IEnumerable<Position> GetPositions(PieceColor pieceColor);

        IEnumerable<Position> GetPositions(PieceColor pieceColor, PieceType pieceType);

        IBoard SetPosition(Position position);

        IEnumerable<Position> GetAttackers(ulong square, PieceColor color);

        int Score { get; }

        bool IsChecked { get; }

        Counters Counters { get; }
    }
}
