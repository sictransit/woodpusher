using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Engine;

namespace SicTransit.Woodpusher
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Logging.EnableLogging(Serilog.Events.LogEventLevel.Debug, false);

            Action<string> consoleOutput = new(s =>
            {
                Console.WriteLine(s);

                Log.Information($"Sent: {s}");
            });

            var uci = new UniversalChessInterface(consoleOutput, new Patzer());

            while (!uci.Quit)
            {
                var line = Console.ReadLine();

                Log.Information($"Received: {line}");

                if (line != null)
                {
                    uci.ProcessCommand(line);
                }
            }
        }
    }
}