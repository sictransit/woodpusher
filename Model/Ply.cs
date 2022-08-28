namespace SicTransit.Woodpusher.Model
{
    public struct Ply
    {
        public Ply(Position position, Move move)
        {
            Position = position;
            Move = move;
        }

        public Position Position { get; }
        public Move Move { get; }

        public override string ToString()
        {
            return $"{Position}{Move}";
        }
    }
}
