using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace ServiceFabric.Utilities.Actors
{
    public class ActorStateEntryDictionary<T>
    {
        public static async Task<ActorStateEntryDictionary<T>> CreateAsync(IActorStateManager stateManager, string name)
        {
            var prefix = name + Separator;
            var stateNames = await stateManager.GetStateNamesAsync().ConfigureAwait(false);
            var dictionary = new ActorStateEntryDictionary<T>(stateManager, prefix);
            foreach (var stateName in stateNames)
            {
                if (stateName.StartsWith(prefix))
                {
                    var key = stateName.Substring(prefix.Length);
                    dictionary.Add(key);
                }
            }
            return dictionary;
        }

        private const string Separator = "\t";
        private readonly string _prefix;
        private readonly IActorStateManager _stateManager;
        private readonly Dictionary<string, ActorStateEntry<T>> _dictionary;

        public ICollection<string> Keys => _dictionary.Keys;

        public ICollection<ActorStateEntry<T>> Values => _dictionary.Values;

        public int Count => _dictionary.Count;

        private ActorStateEntryDictionary(IActorStateManager stateManager, string prefix)
        {
            _stateManager = stateManager;
            _prefix = prefix;
            _dictionary = new Dictionary<string, ActorStateEntry<T>>();
        }

        private string ToStateName(string key) => _prefix + key;

        public ActorStateEntry<T> Add(string key)
        {
            var stateName = ToStateName(key);
            var state = new ActorStateEntry<T>(_stateManager, stateName);
            _dictionary.Add(key, state);
            return state;
        }

        public bool ContainsKey(string key) => _dictionary.ContainsKey(ToStateName(key));

        public async Task<bool> RemoveAsync(string key)
        {
            var stateName = ToStateName(key);
            var removed = await _stateManager.TryRemoveStateAsync(stateName).ConfigureAwait(false);
            _dictionary.Remove(key);
            return removed;
        }

        public bool TryGetValue(string key, out ActorStateEntry<T> value) => _dictionary.TryGetValue(key, out value);

        public bool GetOrCreate(string key, out ActorStateEntry<T> value)
        {
            if (_dictionary.TryGetValue(key, out value))
            {
                return true;
            }
            else
            {
                value = Add(key);
                return false;
            }
        }

        public async Task ClearAsync()
        {
            foreach (var key in _dictionary.Keys)
            {
                var stateName = ToStateName(key);
                await _stateManager.TryRemoveStateAsync(stateName).ConfigureAwait(false);
            }
            _dictionary.Clear();
        }

        public ActorStateEntry<T> this[string key]
        {
            get => _dictionary[key];
        }
    }
}
