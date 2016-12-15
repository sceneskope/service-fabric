using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace SceneSkope.ServiceFabric.Utilities
{
    public class BaseServiceProxy<TService> where TService : IService
    {
        private List<TService> _allRoPartitionsProxyList;
        private List<TService> _allRwPartitionsProxyList;
        private readonly string _listenerName;
        private readonly Uri _uri;

        protected BaseServiceProxy(string name, string listenerName = null)
        {
            _uri = new Uri(name);
            _listenerName = listenerName;
        }

        protected Task<List<TService>> AllRoPartitionsProxy => GetServiceListAsync(_allRoPartitionsProxyList, list => _allRoPartitionsProxyList = list, TargetReplicaSelector.RandomReplica);
        protected Task<List<TService>> AllRwPartitionsProxy => GetServiceListAsync(_allRoPartitionsProxyList, list => _allRwPartitionsProxyList = list, TargetReplicaSelector.Default);

        private async Task<List<TService>> GetServiceListAsync(List<TService> list, Action<List<TService>> setter, TargetReplicaSelector selector)
        {
            if (list == null)
            {
                list = await PartitionUtilities.GetServiceListAsync<TService>(_uri, selector);
                setter(list);
            }
            return list;
        }

        protected async Task<T> ServiceFor<T>(ServicePartitionKey key, Func<TService, Task<T>> func)
        {
            var service = ServiceProxy.Create<TService>(_uri, key, listenerName: _listenerName);
            try
            {
                return await func(service);
            }
            catch (AggregateException ex)
            {
                var flattened = ex.Flatten();
                throw ex.InnerException;
            }
        }

        protected async Task ServiceFor(ServicePartitionKey key, Func<TService, Task> func)
        {
            var service = ServiceProxy.Create<TService>(_uri, key, listenerName: _listenerName);
            try
            {
                await func(service);
            }
            catch (AggregateException ex)
            {
                var flattened = ex.Flatten();
                throw ex.InnerException;
            }
        }

        protected async Task<T> ReadOnlyServiceFor<T>(ServicePartitionKey key, Func<TService, Task<T>> func)
        {
            var service = ServiceProxy.Create<TService>(_uri, key, listenerName: _listenerName, targetReplicaSelector: TargetReplicaSelector.RandomReplica);
            try
            {
                return await func(service);
            }
            catch (AggregateException ex)
            {
                var flattened = ex.Flatten();
                throw ex.InnerException;
            }
        }

        protected async Task ReadOnlyServiceFor(ServicePartitionKey key, Func<TService, Task> func)
        {
            var service = ServiceProxy.Create<TService>(_uri, key, listenerName: _listenerName, targetReplicaSelector: TargetReplicaSelector.RandomReplica);
            try
            {
                await func(service);
            }
            catch (AggregateException ex)
            {
                var flattened = ex.Flatten();
                throw ex.InnerException;
            }
        }

        protected static async Task<T[]> CollectFromAllServicesAsync<T>(IList<TService> services, Func<TService, Task<T>> serviceFunc)
        {
            try
            {
                var tasks = new List<Task<T>>(services.Count);
                foreach (var service in services)
                {
                    tasks.Add(serviceFunc(service));
                }
                var results = await Task.WhenAll(tasks);
                return results;
            }
            catch (AggregateException ex)
            {
                var flattened = ex.Flatten();
                throw ex.InnerException;
            }
        }

    }
}
