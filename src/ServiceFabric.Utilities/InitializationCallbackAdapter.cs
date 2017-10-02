using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace ServiceFabric.Utilities
{
    public sealed class InitializationCallbackAdapter
    {
        public Task OnInitialize()
        {
            foreach (var registration in _registrations)
            {
                registration(StateManager);
            }
            return Task.FromResult(true);
        }

        [Obsolete("Uses obsolete state serializer", false)]
        public void RegisterStateSerializer<T>(IStateSerializer<T> serializer) =>
            _registrations.Add(sm => sm.TryAddStateSerializer(serializer));

        private readonly List<Action<IReliableStateManager>> _registrations = new List<Action<IReliableStateManager>>();

        public IReliableStateManager StateManager { get; set; }
    }
}
