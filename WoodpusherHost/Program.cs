using NetMQ;
using NetMQ.Sockets;
using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Engine;
using SicTransit.Woodpusher.Parsing;

namespace SicTransit.Woodpusher
{
    public class Program
    {
        static void Main(string[] args)
        {
            Logging.EnableLogging(Serilog.Events.LogEventLevel.Information);

            var patzer = new Patzer();

            patzer.Initialize(ForsythEdwardsNotation.StartingPosition);

            using (var server = new ResponseSocket("@tcp://localhost:5556"))
            {
                while (true)
                {
                    string message = server.ReceiveFrameString();

                    Log.Information($"Received: {message}");

                    server.SendFrame(message);
                }
            }
        }
    }
}