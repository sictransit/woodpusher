using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Common;

public class LegalMove
{ 
    public LegalMove(Move move, IBoard board)
    {
        Move = move;
        Board = board;
    }

    public Move Move { get; init; }

    public IBoard Board { get; init; }
}