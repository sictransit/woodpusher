namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class TargetExtensions
    {
        public static Move ToMove(this Target target, Position position)
        {
            return new Move(position, target);
        }
    }
}
