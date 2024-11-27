namespace SicTransit.Woodpusher.Common
{
    public class EngineOptions
    {
        public bool UseOpeningBook { get; set; } = false;

        public static EngineOptions Default => new();
    }
}
