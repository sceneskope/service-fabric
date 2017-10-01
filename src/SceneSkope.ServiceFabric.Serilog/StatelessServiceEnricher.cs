using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Events;

namespace SceneSkope.ServiceFabric.Serilog
{
    public class StatelessServiceEnricher : ServiceEnricher<StatelessServiceContext>
    {
        private LogEventProperty _instanceId;

        public StatelessServiceEnricher(StatelessServiceContext context) : base(context)
        {
        }

        public override void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            base.Enrich(logEvent, propertyFactory);
            _instanceId = _instanceId ?? propertyFactory.CreateProperty("instanceId", Context.InstanceId);
            logEvent.AddPropertyIfAbsent(_instanceId);
        }
    }
}
