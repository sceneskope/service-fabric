using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using Serilog;

namespace SceneSkope.ServiceFabric.Serilog
{
    public class SerilogStatelessService : StatelessService
    {
        protected ILogger Log { get; }

        protected SerilogStatelessService(StatelessServiceContext serviceContext, ILogger logger) : base(serviceContext)
        {
            Log = logger.ForContext(new[] {  new StatelessServiceEnricher(serviceContext)});
        }
    }
}
