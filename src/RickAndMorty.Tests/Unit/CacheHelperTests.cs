using Microsoft.Extensions.Caching.Memory;
using RickAndMorty.Web.CrossCutting;

namespace RickAndMorty.Tests.Unit;

public sealed class CacheHelperTests : IDisposable
{
    private readonly IMemoryCache _cache;

    public CacheHelperTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public void Dispose()
    {
        _cache.Dispose();
    }

    [Fact]
    public void Set_StoresValueInCache()
    {
        const string key = "test-key";
        const string value = "test-value";

        _cache.Set(key, value, TimeSpan.FromMinutes(5));

        var result = _cache.TryGetValue(key, out string? cachedValue);
        Assert.True(result);
        Assert.Equal(value, cachedValue);
    }

    [Fact]
    public void Set_StoresComplexObjectInCache()
    {
        const string key = "complex-key";
        var value = new List<string> { "item1", "item2", "item3" };

        _cache.Set(key, value, TimeSpan.FromMinutes(5));

        var result = _cache.TryGetValue(key, out List<string>? cachedValue);
        Assert.True(result);
        Assert.Equal(3, cachedValue!.Count);
        Assert.Equal(value, cachedValue);
    }

    [Fact]
    public void RemoveByPrefix_RemovesAllKeysWithPrefix()
    {
        _cache.Set("Characters:All", "value1", TimeSpan.FromMinutes(5));
        _cache.Set("Characters:Filtered", "value2", TimeSpan.FromMinutes(5));
        _cache.Set("Episodes:All", "value3", TimeSpan.FromMinutes(5));

        _cache.RemoveByPrefix("Characters");

        Assert.False(_cache.TryGetValue("Characters:All", out _));
        Assert.False(_cache.TryGetValue("Characters:Filtered", out _));
        Assert.True(_cache.TryGetValue("Episodes:All", out _));
    }

    [Fact]
    public void RemoveByPrefix_RemovesOnlyMatchingPrefix()
    {
        _cache.Set("Characters:All", "value1", TimeSpan.FromMinutes(5));
        _cache.Set("Character:Single", "value2", TimeSpan.FromMinutes(5));
        _cache.Set("Episodes:All", "value3", TimeSpan.FromMinutes(5));

        _cache.RemoveByPrefix("Characters");

        Assert.False(_cache.TryGetValue("Characters:All", out _));
        Assert.True(_cache.TryGetValue("Character:Single", out _));
        Assert.True(_cache.TryGetValue("Episodes:All", out _));
    }

    [Fact]
    public void RemoveByPrefix_HandlesNonExistentPrefix()
    {
        _cache.Set("Characters:All", "value1", TimeSpan.FromMinutes(5));

        // Should not throw
        _cache.RemoveByPrefix("NonExistent");

        Assert.True(_cache.TryGetValue("Characters:All", out _));
    }

    [Fact]
    public void RemoveByPrefix_HandlesEmptyCache()
    {
        // Should not throw on empty cache
        _cache.RemoveByPrefix("AnyPrefix");

        Assert.True(true); // If we get here, no exception was thrown
    }

    [Fact]
    public void Set_WithExpiration_ExpiresAfterTime()
    {
        const string key = "expiring-key";
        const string value = "expiring-value";

        _cache.Set(key, value, TimeSpan.FromMilliseconds(100));

        // Should be available immediately
        Assert.True(_cache.TryGetValue(key, out string? _));

        // Wait for expiration
        Thread.Sleep(150);

        // Should be expired
        Assert.False(_cache.TryGetValue(key, out string? _));
    }

    [Fact]
    public void RemoveByPrefix_RemovesMultipleKeys()
    {
        for (int i = 0; i < 10; i++)
        {
            _cache.Set($"Test:Key{i}", $"value{i}", TimeSpan.FromMinutes(5));
        }

        _cache.RemoveByPrefix("Test");

        for (int i = 0; i < 10; i++)
        {
            Assert.False(_cache.TryGetValue($"Test:Key{i}", out _));
        }
    }

    [Fact]
    public void Set_OverwritesExistingKey()
    {
        const string key = "overwrite-key";

        _cache.Set(key, "old-value", TimeSpan.FromMinutes(5));
        _cache.Set(key, "new-value", TimeSpan.FromMinutes(5));

        var result = _cache.TryGetValue(key, out string? cachedValue);
        Assert.True(result);
        Assert.Equal("new-value", cachedValue);
    }
}
