namespace SicTransit.Woodpusher.Common.Exceptions
{
    public sealed class FENParsingException : Exception
    {
        public FENParsingException(string fen, string message) : base($"Unable to parse [{fen}]: {message}")
        { }

        public FENParsingException(string message, Exception inner) : base(message, inner)
        { }
    }
}
