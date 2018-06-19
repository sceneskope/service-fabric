using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Utilities
{
    public class TimeoutHandler
    {
        public int MinDelayMs { get; set; } = 100;
        public int MaxDelayMs { get; set; } = 2000;
        public int MaxRetryCount { get; set; } = int.MaxValue;

        private readonly Random _rand = new Random();

        public Task HandleTimeout(Func<Task> handler, CancellationToken ct, Action<int> beforeDelayAction = null) =>
            HandleTimeout(() => handler().ContinueWith(_ => false), ct, beforeDelayAction);

        public async Task<T> HandleTimeout<T>(Func<Task<T>> handler, CancellationToken ct, Action<int> beforeDelayAction = null)
        {
            var retryCount = MaxRetryCount;
            while (retryCount-- > 0)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    return await handler().ConfigureAwait(false);
                }
                catch (TimeoutException)
                {
                    if (retryCount == 0)
                    {
                        throw;
                    }
                    ct.ThrowIfCancellationRequested();
                    var delayMs = _rand.Next(MinDelayMs, MaxDelayMs);
                    beforeDelayAction?.Invoke(delayMs);
                    await Task.Delay(delayMs, ct).ConfigureAwait(false);
                }
            }
            throw new TimeoutException("Retries exceeded");
        }
    }
}
