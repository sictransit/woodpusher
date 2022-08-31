namespace SicTransit.Woodpusher.Common.Exceptions
{
    public sealed class PGNParsingException : Exception
    {
        public PGNParsingException(string pgn, string message) : base($"Unable to parse [{pgn}]: {message}")
        { }

        public PGNParsingException(string message, Exception inner) : base(message, inner)
        { }
    }
}
