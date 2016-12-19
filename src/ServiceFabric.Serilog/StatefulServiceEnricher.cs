using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Events;

namespace ServiceFabric.Serilog
{
    public class StatefulServiceEnricher : ServiceEnricher<StatefulServiceContext>
    {
        private LogEventProperty _replicaId;

        public StatefulServiceEnricher(StatefulServiceContext context) : base(context)
        {
        }

        public override void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            base.Enrich(logEvent, propertyFactory);
            if (_replicaId == null) _replicaId = propertyFactory.CreateProperty("replicaId", Context.ReplicaId);
            logEvent.AddPropertyIfAbsent(_replicaId);
        }
    }
}
