namespace SicTransit.Woodpusher.Common.Parsing.Exceptions
{
    public sealed class FenParsingException : Exception
    {
        public FenParsingException(string fen, string message) : base($"Unable to parse [{fen}]: {message}")
        { }
    }
}
