using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SceneSkope.ServiceFabric.Utilities;
using Serilog;
using Serilog.Events;

namespace SceneSkope.ServiceFabric.Seq
{
    public static class ServiceFabricLogger
    {
        private static ILogger CreaterDefaultLogger()
        {
            var configurationProvider = new FabricConfigurationProvider("SeqConfig");

            var loggerConfiguration = new LoggerConfiguration();
            if (configurationProvider.HasConfiguration)
            {
                var seqServer = configurationProvider.GetValue("SeqServer");
                loggerConfiguration =
                    loggerConfiguration
                    .WriteTo.Seq(seqServer, period: TimeSpan.FromMilliseconds(500))
                    ;

                var level = configurationProvider.GetValue("MinimumLevel");
                LogEventLevel minimumLevel;
                if (!string.IsNullOrWhiteSpace(level) && Enum.TryParse<LogEventLevel>(level, true, out minimumLevel))
                {
                    loggerConfiguration = loggerConfiguration.MinimumLevel.Is(minimumLevel);
                }
            }
            else
            {
                loggerConfiguration =
                    loggerConfiguration
                    .MinimumLevel.Error()
                    ;
            }

            Log.Logger = loggerConfiguration.CreateLogger();
            return Log.Logger;
        }

        public static ILogger Logger { get; } = CreaterDefaultLogger();
    }
}
