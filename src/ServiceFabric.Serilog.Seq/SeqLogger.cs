using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceFabric.Utilities;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ServiceFabric.Serilog.Seq
{
    public static class SeqLogger
    {
        public static ILogger DefaultLogger { get; } = CreaterDefaultLogger();

        private static ILogger CreaterDefaultLogger()
        {
            var configurationProvider = new FabricConfigurationProvider("SeqConfig");
            return CreateLogger(configurationProvider);
        }

        public static ILogger CreateLogger(IConfigurationProvider configurationProvider)
        {
            var loggerConfiguration = CreateLoggerConfiguration(configurationProvider);
            return loggerConfiguration.CreateLogger();
        }

        public static LoggerConfiguration CreateDefaultLoggerConfiguration()
        {
            var configurationProvider = new FabricConfigurationProvider("SeqConfig");
            return CreateLoggerConfiguration(configurationProvider);
        }

        public static LoggerConfiguration CreateLoggerConfiguration(IConfigurationProvider configurationProvider)
        {
            var loggerConfiguration = new LoggerConfiguration();
            if (configurationProvider.HasConfiguration)
            {
                var level = configurationProvider.GetValue("MinimumLevel");
                if (!string.IsNullOrWhiteSpace(level) && Enum.TryParse<LogEventLevel>(level, true, out var minimumLevel))
                {
                    loggerConfiguration = loggerConfiguration.MinimumLevel.Is(minimumLevel);
                }
                else
                {
                    minimumLevel = LogEventLevel.Information;
                }

                var levelSwitch = new LoggingLevelSwitch(minimumLevel);
                var seqServer = configurationProvider.GetValue("SeqServer");
                var apiKey = configurationProvider.TryGetValue("ApiKey");
                loggerConfiguration =
                    loggerConfiguration
                    .WriteTo.Seq(seqServer,
                        period: TimeSpan.FromMilliseconds(500),
                        compact: true,
                        apiKey: apiKey,
                        controlLevelSwitch: levelSwitch);
            }
            return loggerConfiguration;
        }
    }
}
