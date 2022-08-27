namespace SicTransit.Woodpusher.Model.Exceptions
{
    public sealed class FenParsingException : Exception
    {
        public FenParsingException(string fen, string message) : base($"Unable to parse [{fen}]: {message}")
        { }

        public FenParsingException(string message, Exception inner) : base(message, inner)
        { }
    }
}
