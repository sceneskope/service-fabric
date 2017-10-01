using Microsoft.ServiceFabric.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.Utilities
{
    public class LazyReliableState<T> where T : IReliableState
    {
        public IReliableStateManager StateManager { get; }
        public string Name { get; }
        public LazyReliableState(IReliableStateManager stateManager, string name)
        {
            StateManager = stateManager;
            Name = name;
        }

        private T _entry;

        public Task<T> GetOrCreateAsync()
        {
            if (_entry != null)
            {
                return Task.FromResult(_entry);
            }
            else
            {
                return CreateAsync();
            }
        }

        private async Task<T> CreateAsync()
        {
            var entry = await StateManager.GetOrAddAsync<T>(Name).ConfigureAwait(false);
            _entry = entry;
            return entry;
        }
    }
}
