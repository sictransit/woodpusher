using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Model.Interfaces
{
    public interface IBoard
    {
        IEnumerable<Position> GetPositions(PieceColour colour);

        IEnumerable<Position> GetPositions(PieceColour colour, PieceType type, int file);

        Square FindKing(PieceColour colour);

        bool IsOccupied(Square square, PieceColour colour);

        bool IsOccupied(Square square);

        PieceColour ActiveColour { get; }

        Castlings Castlings { get; }

        Square? EnPassantTarget { get; }
    }
}
