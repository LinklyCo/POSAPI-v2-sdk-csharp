using System.Collections.Generic;

namespace Linkly.PosApi.Sdk.Common
{
    internal static class DictionaryExtensions
    {
        public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
            where TKey : notnull
            where TValue : class =>
            dict.TryGetValue(key, out var value) ? value : default;
    }
}