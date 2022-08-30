using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Engine;

namespace SicTransit.Woodpusher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Logging.EnableLogging(Serilog.Events.LogEventLevel.Information);

            var patzer = new Patzer();

            patzer.Initialize(FEN.StartingPosition);

            foreach (var ply in patzer.GetValidPly())
            {
                Log.Debug(ply.ToString());
            }
        }
    }
}