﻿using NetMQ;
using NetMQ.Sockets;
using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Engine;
using SicTransit.Woodpusher.Parsing;
using System.Diagnostics;

namespace SicTransit.Woodpusher
{
    public class Program
    {
        static void Main(string[] args)
        {
            Logging.EnableLogging(Serilog.Events.LogEventLevel.Information);

            using (var client = new RequestSocket(">tcp://localhost:5556"))
            {
                
                while (true)
                {
                    var line = Console.ReadLine();

                    client.SendFrame(line!);

                    string response = client.ReceiveFrameString();

                    Log.Information($"Received: {response}");                    
                }
            }
        }
    }
}