namespace SicTransit.Woodpusher.Model
{
    public struct Ply
    {
        public Ply(Position position, Target target)
        {
            Position = position;
            Target = target;
        }

        public Position Position { get; }
        public Target Target { get; }

        public override string ToString()
        {
            return $"{Position}{Target}";
        }
    }
}
