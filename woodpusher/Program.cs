using NetMQ;
using NetMQ.Sockets;
using Serilog;
using SicTransit.Woodpusher.Common;

namespace SicTransit.Woodpusher
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Logging.EnableLogging(Serilog.Events.LogEventLevel.Information);

            using var client = new RequestSocket(">tcp://localhost:5556");
            while (true)
            {
                var line = Console.ReadLine();

                client.SendFrame(line!);

                var response = client.ReceiveFrameString();

                Log.Information($"Received: {response}");
            }
        }
    }
}