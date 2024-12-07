using Serilog;
using SicTransit.Woodpusher.Common;

namespace SicTransit.Woodpusher
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Logging.EnableLogging(Serilog.Events.LogEventLevel.Debug, false);

            static void ConsoleOutput(string message, bool isInfo = false)
            {
                Console.WriteLine(message);

                if (isInfo)
                {
                    Log.Debug("Sent: {Message}", message);
                }
                else
                {
                    Log.Information("Sent: {Message}", message);
                }
            }

            using var quitSource = new CancellationTokenSource();

            var uci = new UniversalChessInterface(ConsoleOutput, quitSource.Token);

            uci.Run();

            while (!quitSource.Token.IsCancellationRequested)
            {
                var line = Console.ReadLine()?.Trim();

                Log.Information("Received: {Line}", line);

                if (!string.IsNullOrEmpty(line))
                {
                    switch (line)
                    {
                        case "quit":
                            quitSource.Cancel();
                            break;
                        case "stop":
                            uci.Stop();
                            break;
                        default:
                            uci.EnqueueCommand(line);
                            break;
                    }
                }
            }
        }
    }
}