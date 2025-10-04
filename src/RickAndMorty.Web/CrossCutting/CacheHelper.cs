using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace RickAndMorty.Web.CrossCutting;

public static class CacheHelper
{
    private static readonly ConcurrentDictionary<string, byte> CacheKeys = new();

    public static void Set<T>(this IMemoryCache cache, string key, T value, TimeSpan expiration)
    {
        CacheKeys.TryAdd(key, 0);
        
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration,
            PostEvictionCallbacks =
            {
                new PostEvictionCallbackRegistration
                {
                    EvictionCallback = (k, v, r, s) => CacheKeys.TryRemove(k.ToString()!, out _)
                }
            }
        };
        
        cache.Set(key, value, options);
    }

    public static void RemoveByPrefix(this IMemoryCache cache, string prefix)
    {
        var keysToRemove = CacheKeys.Keys
            .Where(k => k.StartsWith(prefix))
            .ToList();

        foreach (var key in keysToRemove)
        {
            cache.Remove(key);
            CacheKeys.TryRemove(key, out _);
        }
    }
}