namespace SicTransit.Woodpusher.Parsing.Exceptions
{
    public sealed class PGNParsingExceptionQ : Exception
    {
        public PGNParsingExceptionQ(string pgn, string message) : base($"Unable to parse [{pgn}]: {message}")
        { }

        public PGNParsingExceptionQ(string message, Exception inner) : base(message, inner)
        { }
    }
}
