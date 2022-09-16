using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Engine;
using SicTransit.Woodpusher.Parsing;
using System.Diagnostics;

namespace SicTransit.Woodpusher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Logging.EnableLogging(Serilog.Events.LogEventLevel.Information);

            var patzer = new Patzer();

            patzer.Initialize(ForsythEdwardsNotation.StartingPosition);

            foreach (var move in patzer.GetValidMoves(patzer.Board.ActiveColour))
            {
                Log.Debug(move.ToString());

                Debugger.Launch();
            }
        }
    }
}