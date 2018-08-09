using System;
using System.Fabric;
using System.Threading.Tasks;

namespace ServiceFabric.Utilities
{
    public class ServiceStateProvider<TState>
        where TState : class
    {
        private TState _activeState;

        public TState Active
        {
            get
            {
                var active = _activeState;
                if (active == null)
                {
                    throw new FabricTransientException("Not ready");
                }
                else
                {
                    return active;
                }
            }
        }

        public async Task<IDisposable> CreateAsync(Func<Task<TState>> asyncCreator)
        {
            var state = await asyncCreator().ConfigureAwait(false);
            return new Disposer(this, state);
        }

        private class Disposer : IDisposable
        {
            private readonly ServiceStateProvider<TState> _serviceStateProvider;

            public void Dispose()
            {
                _serviceStateProvider._activeState = default;
            }

            public Disposer(ServiceStateProvider<TState> serviceStateProvider, TState state)
            {
                _serviceStateProvider = serviceStateProvider;
                serviceStateProvider._activeState = state;
            }
        }
    }
}
