using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SceneSkope.ServiceFabric.Utilities;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace SceneSkope.ServiceFabric.Seq
{
    public static class ServiceFabricLogger
    {
        private static ILogger CreaterDefaultLogger()
        {
            var configurationProvider = new FabricConfigurationProvider("SeqConfig");
            Log.Logger = CreateLogger(configurationProvider);
            return Log.Logger;
        }

        public static ILogger CreateLogger(IConfigurationProvider configurationProvider)
        {
            var loggerConfiguration = CreateLoggerConfiguration(configurationProvider);
            return loggerConfiguration.CreateLogger();

        }

        public static LoggerConfiguration CreateLoggerConfiguration(IConfigurationProvider configurationProvider)
        {
            var loggerConfiguration = new LoggerConfiguration();
            if (configurationProvider.HasConfiguration)
            {
                var level = configurationProvider.GetValue("MinimumLevel");
                LogEventLevel minimumLevel;
                if (!string.IsNullOrWhiteSpace(level) && Enum.TryParse<LogEventLevel>(level, true, out minimumLevel))
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
            else
            {
                loggerConfiguration =
                    loggerConfiguration
                    .MinimumLevel.Error()
                    ;
            }
            return loggerConfiguration;
        }

        public static ILogger Logger { get; } = CreaterDefaultLogger();
    }
}
