using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using Serilog;

namespace SceneSkope.ServiceFabric.Seq
{
    public static class ServiceLogger
    {
        public static ILogger CreateLogger(this StatefulServiceBase service) =>
            ServiceFabricLogger.Logger.ForContext(new[] { new StatefulServiceEnricher(service.Context) });

        public static ILogger CreateLogger(this StatelessService service) =>
            ServiceFabricLogger.Logger.ForContext(new[] { new StatelessServiceEnricher(service.Context) });
    }
}
