namespace SicTransit.Woodpusher.Parsing.Moves
{
    public abstract class PgnMove
    {
        public abstract bool TryParse(string s, out PgnMove move);
    }
}
