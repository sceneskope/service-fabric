using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace ServiceFabric.Utilities
{
    public static class PartitionUtilities
    {
        public static ServicePartitionKey ServicePartitionKey(this ServicePartitionInformation information)
        {
            switch (information.Kind)
            {
                case ServicePartitionKind.Int64Range: return new ServicePartitionKey(((Int64RangePartitionInformation)information).LowKey);
                case ServicePartitionKind.Named: return new ServicePartitionKey(((NamedPartitionInformation)information).Name);
                case ServicePartitionKind.Singleton: return new ServicePartitionKey();
                default: throw new ArgumentException("Invalid partition", nameof(information));
            }
        }

        public static async Task<List<ServicePartitionInformation>> GetPartitionListAsync(Uri serviceName)
        {
            using (var fabricClient = new FabricClient())
            {
                var partitions = await fabricClient.QueryManager.GetPartitionListAsync(serviceName);
                var result = partitions.Select(p => p.PartitionInformation).ToList();
                return result;
            }
        }

        public static async Task<List<ServicePartitionInformation>> GetOrderedPartitionListAsync(Uri serviceName)
        {
            var partitions = await GetPartitionListAsync(serviceName);
            if (partitions.Count > 1)
            {
                switch (partitions[0].Kind)
                {
                    case ServicePartitionKind.Int64Range:
                        partitions = partitions.OrderBy(i => ((Int64RangePartitionInformation)i).LowKey).ToList();
                        break;

                    case ServicePartitionKind.Named:
                        partitions = partitions.OrderBy(i => ((NamedPartitionInformation)i).Name).ToList();
                        break;
                }
            }
            return partitions;
        }

        public static async Task<List<TService>> GetServiceListAsync<TService>(Uri serviceName, TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.Default, string listenerName = null)
            where TService : IService =>
            (await GetPartitionListAsync(serviceName))
            .Select(spi => ServiceProxy.Create<TService>(serviceName, spi.ServicePartitionKey(), targetReplicaSelector, listenerName))
            .ToList();

        public static async Task<List<TService>> GetOrderedServiceListAsync<TService>(Uri serviceName, TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.Default, string listenerName = null)
            where TService : IService =>
            (await GetOrderedPartitionListAsync(serviceName))
            .Select(spi => ServiceProxy.Create<TService>(serviceName, spi.ServicePartitionKey(), targetReplicaSelector, listenerName))
            .ToList();

    }
}
