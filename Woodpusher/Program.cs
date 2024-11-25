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

            static void ConsoleOutput(string s)
            {
                Console.WriteLine(s);

                Log.Information("Sent: {Message}", s);
            }

            var uci = new UniversalChessInterface(ConsoleOutput, new Patzer(ConsoleOutput));

            while (!uci.Quit)
            {
                var line = Console.ReadLine();

                Log.Information("Received: {Line}", line);

                if (line != null)
                {
                    uci.ProcessCommand(line);
                }
            }
        }
    }
}