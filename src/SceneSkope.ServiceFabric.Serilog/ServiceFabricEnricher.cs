using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Events;

namespace SceneSkope.ServiceFabric.Serilog
{
    public class ServiceFabricEnricher<T> : ILogEventEnricher where T : ServiceContext
    {
        protected T Context { get; }
        private LogEventProperty _nodeName;

        public ServiceFabricEnricher(T context)
        {
            Context = context;
        }

        public virtual void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            _nodeName = _nodeName ?? propertyFactory.CreateProperty("nodeName", Context.NodeContext.NodeName);
            logEvent.AddPropertyIfAbsent(_nodeName);
        }
    }
}
