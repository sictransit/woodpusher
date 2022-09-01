using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Engine;
using SicTransit.Woodpusher.Parsing;

namespace SicTransit.Woodpusher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Logging.EnableLogging(Serilog.Events.LogEventLevel.Information);

            var patzer = new Patzer();

            patzer.Initialize(ForsythEdwardsNotation.StartingPosition);

            foreach (var ply in patzer.GetValidPly())
            {
                Log.Debug(ply.ToString());
            }
        }
    }
}