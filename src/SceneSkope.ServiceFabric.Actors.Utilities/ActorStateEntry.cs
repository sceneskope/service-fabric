using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;

namespace SceneSkope.ServiceFabric.Actors.Utilities
{
    public class ActorStateEntry<T>
    {
        private readonly string _name;
        private readonly IActorStateManager _stateManager;

        public ActorStateEntry(IActorStateManager stateManager, string name)
        {
            _stateManager = stateManager;
            _name = name;
        }

        public Task<T> GetAsync() => _stateManager.GetStateAsync<T>(_name);
        public Task SetAsync(T value) => _stateManager.SetStateAsync(_name, value);
        public Task<ConditionalValue<T>> TryGetAsync() => _stateManager.TryGetStateAsync<T>(_name);
        public async Task<ConditionalValue<T>> TryRemoveAsync()
        {
            var value = await _stateManager.TryGetStateAsync<T>(_name);
            if (value.HasValue)
            {
                await _stateManager.RemoveStateAsync(_name);
            }
            return value;
        }

        public async Task<T> GetOrCreateAsync(Func<T> factory)
        {
            var value = await _stateManager.TryGetStateAsync<T>(_name);
            if (value.HasValue)
            {
                return value.Value;
            }
            else
            {
                var newValue = factory();
                await _stateManager.SetStateAsync(_name, newValue);
                return newValue;
            }
        }

        public Task RemoveAsync() => _stateManager.RemoveStateAsync(_name);

        public Task<bool> ExistsAsync() => _stateManager.ContainsStateAsync(_name);
    }
}
