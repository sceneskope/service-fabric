using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneSkope.ServiceFabric.Utilities
{
    public static class ReliableQueueExtensions
    {
        public static async Task<List<T>> TryReadEntriesAsync<T>(this IReliableQueue<T> queue, IReliableStateManager stateManager, int maxCount)
        {
            List<T> results = null;

            using (var tx = stateManager.CreateTransaction())
            {
                do
                {
                    var entry = await queue.TryDequeueAsync(tx).ConfigureAwait(false);
                    if (!entry.HasValue)
                    {
                        break;
                    }
                    results = results ?? new List<T>(maxCount);
                    results.Add(entry.Value);
                } while (results.Count < maxCount);

                tx.Abort();
            }
            return results;
        }

        public static async Task ClearEntriesAsync<T>(this IReliableQueue<T> queue, IReliableStateManager stateManager, int count)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                await ClearEntriesAsync(queue, tx, count).ConfigureAwait(false);
                await tx.CommitAsync().ConfigureAwait(false);
            }
        }

        public static async Task ClearEntriesAsync<T>(this IReliableQueue<T> queue, ITransaction tx, int count)
        {
            while (count-- > 0)
            {
                await queue.TryDequeueAsync(tx).ConfigureAwait(false);
            }
        }
    }
}
