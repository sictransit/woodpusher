namespace SicTransit.Woodpusher.Model
{
    public struct Ply
    {
        public Ply(Position position, Target move)
        {
            Position = position;
            Move = move;
        }

        public Position Position { get; }
        public Target Move { get; }

        public override string ToString()
        {
            return $"{Position}{Move}";
        }
    }
}
