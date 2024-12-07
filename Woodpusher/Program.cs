using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Engine;
using System.Collections.Concurrent;

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