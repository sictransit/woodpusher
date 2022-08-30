using Serilog;
using Serilog.Events;
using System.Reflection;

namespace SicTransit.Woodpusher.Common
{
    public static class Logging
    {
        public static void EnableLogging(LogEventLevel level = LogEventLevel.Information)
        {
            var logFilename = $"{Assembly.GetCallingAssembly().GetName().Name}.log";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(level)
                .WriteTo.File(logFilename, LogEventLevel.Information)
                .CreateLogger();
        }

        public static void EnableUnitTestLogging(LogEventLevel level = LogEventLevel.Information)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Trace(level)
                .CreateLogger();
        }
    }
}