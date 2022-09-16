using NetMQ;
using NetMQ.Sockets;
using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Engine;
using SicTransit.Woodpusher.Parsing;

namespace SicTransit.Woodpusher
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Logging.EnableLogging();

            var patzer = new Patzer();

            patzer.Initialize(ForsythEdwardsNotation.StartingPosition);

            using var server = new ResponseSocket("@tcp://localhost:5556");

            while (true)
            {
                var message = server.ReceiveFrameString();

                Log.Information($"Received: {message}");

                server.SendFrame(message);

                Log.Information($"Sent: {message}");
            }
        }
    }
}