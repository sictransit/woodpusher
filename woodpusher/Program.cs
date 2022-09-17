using Serilog;
using SicTransit.Woodpusher.Common;

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

            var uci = new UniversalChessInterface();

            uci.RegisterConsoleCallback(consoleOutput);

            while (!uci.Stop)
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