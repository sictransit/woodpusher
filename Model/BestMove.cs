namespace SicTransit.Woodpusher.Model
{
    public class BestMove
    {
        public BestMove(AlgebraicMove move, AlgebraicMove? ponder = null)
        {
            Move = move;
            Ponder = ponder;
        }

        public AlgebraicMove Move { get; }
        public AlgebraicMove? Ponder { get; }
    }
}
