using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace SceneSkope.ServiceFabric.Utilities
{
    public static class ReliableDictionaryExtensions
    {
        public static Task<List<TValue>> FilterDictionaryAsync<TKey, TValue>(this IReliableDictionary<TKey, TValue> dictionary, ITransaction tx, Func<TKey, TValue, bool> filter, CancellationToken cancel)
            where TKey : IComparable<TKey>, IEquatable<TKey> => FilterDictionaryAsync(dictionary, tx, filter, (k, v) => v, cancel);

        public static Task<List<TValue>> FilterDictionaryAsync<TKey, TValue>(this IReliableDictionary<TKey, TValue> dictionary, ITransaction tx, CancellationToken cancel)
            where TKey : IComparable<TKey>, IEquatable<TKey> => FilterDictionaryAsync(dictionary, tx, (k, v) => true, (k, v) => v, cancel);

        public static async Task<List<TResult>> FilterDictionaryAsync<TKey, TValue, TResult>(this IReliableDictionary<TKey, TValue> dictionary, ITransaction tx, Func<TKey, TValue, bool> filter, Func<TKey, TValue, TResult> convert, CancellationToken cancel)
            where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            var result = new List<TResult>();
            var enumerable = await dictionary.CreateEnumerableAsync(tx).ConfigureAwait(false);
            using (var enumerator = enumerable.GetAsyncEnumerator())
            {
                while (await enumerator.MoveNextAsync(cancel).ConfigureAwait(false))
                {
                    var current = enumerator.Current;
                    if (filter(current.Key, current.Value))
                    {
                        var converted = convert(current.Key, current.Value);
                        result.Add(converted);
                    }
                }
            }
            return result;
        }
    }
}
