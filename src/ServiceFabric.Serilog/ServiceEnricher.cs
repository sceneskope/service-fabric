using Serilog.Core;
using Serilog.Events;
using System.Fabric;

namespace ServiceFabric.Serilog
{
    public class ServiceEnricher<T> : ServiceFabricEnricher<T> where T : ServiceContext
    {
        private LogEventProperty _version;
        private LogEventProperty _serviceName;
        private LogEventProperty _partitionId;
        private LogEventProperty _applicationName;

        public ServiceEnricher(T context) : base(context)
        {
        }

        public override void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            base.Enrich(logEvent, propertyFactory);

            _version = _version ?? propertyFactory.CreateProperty("serviceManifestVersion", Context.CodePackageActivationContext.GetServiceManifestVersion());
            _serviceName = _serviceName ?? propertyFactory.CreateProperty("serviceName", Context.ServiceName);
            _partitionId = _partitionId ?? propertyFactory.CreateProperty("partitionId", Context.PartitionId);
            _applicationName = _applicationName ?? propertyFactory.CreateProperty("applicationName", Context.CodePackageActivationContext.ApplicationName);

            logEvent.AddPropertyIfAbsent(_version);
            logEvent.AddPropertyIfAbsent(_serviceName);
            logEvent.AddPropertyIfAbsent(_partitionId);
            logEvent.AddPropertyIfAbsent(_applicationName);
        }
    }
}
