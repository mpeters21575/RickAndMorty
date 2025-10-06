using RickAndMorty.Web.CrossCutting;

namespace RickAndMorty.Tests.Unit;

public sealed class CacheKeysTests
{
    [Fact]
    public void Characters_HasCorrectValue()
    {
        Assert.Equal("characters_cache", CacheKeys.Characters);
    }

    [Fact]
    public void Episodes_HasCorrectValue()
    {
        Assert.Equal("episodes_cache", CacheKeys.Episodes);
    }

    [Fact]
    public void CacheKeys_AreUsableAsStrings()
    {
        var key1 = $"{CacheKeys.Characters}:All";
        var key2 = $"{CacheKeys.Episodes}:All";

        Assert.Contains("characters_cache", key1);
        Assert.Contains("episodes_cache", key2);
    }
}
