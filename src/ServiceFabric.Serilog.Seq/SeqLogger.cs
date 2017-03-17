using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceFabric.Utilities;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Fabric;
using System.Fabric.Health;

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
                var level = configurationProvider.TryGetValue("MinimumLevel");
                if (string.IsNullOrWhiteSpace(level) || !Enum.TryParse<LogEventLevel>(level, true, out var minimumLevel))
                {
                    minimumLevel = LogEventLevel.Information;
                }

                var periodMilliseconds = configurationProvider.TryGetValue("PeriodMilliseconds");
                if (string.IsNullOrWhiteSpace(periodMilliseconds) || !int.TryParse(periodMilliseconds, out var period))
                {
                    period = 500;
                }

                var levelSwitch = new LoggingLevelSwitch(minimumLevel);
                var seqServer = configurationProvider.GetValue("SeqServer");
                var apiKey = configurationProvider.TryGetValue("ApiKey");
                loggerConfiguration =
                    loggerConfiguration
                    .MinimumLevel.ControlledBy(levelSwitch)
                    .WriteTo.Seq(seqServer,
                        period: TimeSpan.FromMilliseconds(period),
                        compact: true,
                        apiKey: apiKey,
                        controlLevelSwitch: levelSwitch);
            }
            return loggerConfiguration;
        }
    }
}
