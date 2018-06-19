using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;
using Serilog;
using System.Fabric;

namespace ServiceFabric.Serilog
{
    public abstract class SerilogStatefulService : StatefulService
    {
        protected ILogger Log { get; }

        protected SerilogStatefulService(StatefulServiceContext serviceContext, ILogger logger)
            : base(serviceContext)
        {
            Log = logger.ForContext(new[] { new StatefulServiceEnricher(serviceContext) });
        }

        protected SerilogStatefulService(StatefulServiceContext serviceContext, IReliableStateManagerReplica2 reliableStateManagerReplica, ILogger logger)
            : base(serviceContext, reliableStateManagerReplica)
        {
            Log = logger.ForContext(new[] { new StatefulServiceEnricher(serviceContext) });
        }
    }
}
