using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Events;

namespace SceneSkope.ServiceFabric.Seq
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
            if (_nodeName == null) _nodeName = propertyFactory.CreateProperty("NodeName", Context.NodeContext.NodeName);
            logEvent.AddPropertyIfAbsent(_nodeName);
        }
    }
}
