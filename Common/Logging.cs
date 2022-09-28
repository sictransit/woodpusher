using Serilog;
using Serilog.Events;
using System.Reflection;

namespace SicTransit.Woodpusher.Common
{
    public static class Logging
    {
        public static void EnableLogging(LogEventLevel level = LogEventLevel.Information, bool enableConsole = true)
        {
            var logFilename = $"{Assembly.GetCallingAssembly().GetName().Name}.log";

            var loggerConfiguration = new LoggerConfiguration().MinimumLevel.Debug().Enrich.FromLogContext();

            if (enableConsole)
            {
                loggerConfiguration = loggerConfiguration.WriteTo.Console(level);
            }

            Log.Logger = loggerConfiguration
                .WriteTo.File(logFilename, LogEventLevel.Information, shared: true)
                .CreateLogger();
        }

        public static void EnableUnitTestLogging(LogEventLevel level = LogEventLevel.Information)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Trace(level)
                .CreateLogger();
        }
    }
}