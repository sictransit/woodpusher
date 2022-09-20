namespace SicTransit.Woodpusher.Parsing.Exceptions
{
    public sealed class PgnParsingException : Exception
    {
        public PgnParsingException(string pgn, string message) : base($"Unable to parse [{pgn}]: {message}")
        { }

        public PgnParsingException(string message, Exception inner) : base(message, inner)
        { }
    }
}
