using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;
using Serilog;

namespace ServiceFabric.Serilog
{
    public abstract class SerilogStatefulService : StatefulService
    {
        protected ILogger Log { get; }
        protected SerilogStatefulService(StatefulServiceContext serviceContext, ILogger logger) 
            : base(serviceContext)
        {
            Log = logger.ForContext(new[] {  new StatefulServiceEnricher(serviceContext)});
        }

        protected SerilogStatefulService(StatefulServiceContext serviceContext, IReliableStateManagerReplica reliableStateManagerReplica, ILogger logger) 
            : base(serviceContext, reliableStateManagerReplica)
        {
            Log = logger.ForContext(new[] { new StatefulServiceEnricher(serviceContext) });
        }
    }
}
