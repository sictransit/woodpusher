namespace SicTransit.Woodpusher.Parsing.Exceptions
{
    public sealed class FENParsingExceptionQ : Exception
    {
        public FENParsingExceptionQ(string fen, string message) : base($"Unable to parse [{fen}]: {message}")
        { }

        public FENParsingExceptionQ(string message, Exception inner) : base(message, inner)
        { }
    }
}
