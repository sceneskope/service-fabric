using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace SceneSkope.ServiceFabric.Utilities
{
    public static class ServiceFabricHelpers
    {
        public static async Task ForEachAsync<T>(this IAsyncEnumerable<T> enumerable, Action<T> action, CancellationToken cancel)
        {
            using (var enumerator = enumerable.GetAsyncEnumerator())
            {
                while (await enumerator.MoveNextAsync(cancel).ConfigureAwait(false))
                {
                    action(enumerator.Current);
                }
            }
        }

        public static async Task ForEachAsync<T>(this IAsyncEnumerable<T> enumerable, Func<T, CancellationToken, Task> task, CancellationToken cancel)
        {
            using (var enumerator = enumerable.GetAsyncEnumerator())
            {
                while (await enumerator.MoveNextAsync(cancel).ConfigureAwait(false))
                {
                    await task(enumerator.Current, cancel).ConfigureAwait(false);
                }
            }
        }
    }
}
