using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Events;

namespace SceneSkope.ServiceFabric.Serilog
{
    public class ServiceEnricher<T> : ServiceFabricEnricher<T> where T : ServiceContext
    {
        private LogEventProperty _serviceName;
        private LogEventProperty _partitionId;
        private LogEventProperty _applicationName;

        public ServiceEnricher(T context) : base(context)
        {
        }

        public override void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            base.Enrich(logEvent, propertyFactory);

            if (_serviceName == null) _serviceName = propertyFactory.CreateProperty("ServiceName", Context.ServiceName);
            if (_partitionId == null) _partitionId = propertyFactory.CreateProperty("PartitionId", Context.PartitionId);
            if (_applicationName == null) _applicationName = propertyFactory.CreateProperty("ApplicationName", Context.CodePackageActivationContext.ApplicationName);

            logEvent.AddPropertyIfAbsent(_serviceName);
            logEvent.AddPropertyIfAbsent(_partitionId);
            logEvent.AddPropertyIfAbsent(_applicationName);
        }
    }
}
