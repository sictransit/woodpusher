using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Common.Interfaces
{
    public interface IEngine
    {
        Board Board { get; }

        Move GetMove(Position position, Square targetSquare);

        IEnumerable<Move> GetMoves(Position position);

        void Play(Move move);
    }
}
