namespace SicTransit.Woodpusher.Common.Parsing.Exceptions
{
    public sealed class PgnParsingException : Exception
    {
        public PgnParsingException(string pgn, string message) : base($"Unable to parse [{pgn}]: {message}")
        { }
    }
}
