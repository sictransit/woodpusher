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

        public override string ToString()
        {
            return $"{Move}" + (Ponder != null ? $" (pondering {Ponder}) " : string.Empty);
        }
    }
}
