using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RickAndMorty.Domain.Models;
using RickAndMorty.Infrastructure;
using RickAndMorty.Web.Features.GetEpisodes;

namespace RickAndMorty.Tests.Integration;

public sealed class GetEpisodesQueryTests : IAsyncLifetime
{
    private AppDbContext _dbContext = null!;
    private IMemoryCache _cache = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new AppDbContext(options);
        _cache = new MemoryCache(new MemoryCacheOptions());

        await SeedTestDataAsync();
    }

    public Task DisposeAsync()
    {
        _dbContext.Dispose();
        _cache.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAllEpisodes()
    {
        var query = new GetEpisodesQuery(_dbContext, _cache);

        var result = await query.ExecuteAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEpisodesInCorrectOrder()
    {
        var query = new GetEpisodesQuery(_dbContext, _cache);

        var result = await query.ExecuteAsync();

        var ids = result.Select(e => e.Id).ToList();
        Assert.Equal(new[] { 1, 2, 3 }, ids);
    }

    [Fact]
    public async Task ExecuteAsync_MapsAllPropertiesCorrectly()
    {
        var query = new GetEpisodesQuery(_dbContext, _cache);

        var result = await query.ExecuteAsync();

        var episode = result.First();
        Assert.Equal(1, episode.Id);
        Assert.Equal("Pilot", episode.Name);
        Assert.Equal("S01E01", episode.Episode);
        Assert.Equal("December 2, 2013", episode.AirDate);
        Assert.Equal(2, episode.CharacterCount);
    }

    [Fact]
    public async Task ExecuteAsync_CachesResults()
    {
        var query = new GetEpisodesQuery(_dbContext, _cache);

        var firstResult = await query.ExecuteAsync();
        var secondResult = await query.ExecuteAsync();

        Assert.Equal(firstResult.Count, secondResult.Count);
        Assert.Same(firstResult, secondResult); // Should be same instance from cache
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCachedData_WhenAvailable()
    {
        var query = new GetEpisodesQuery(_dbContext, _cache);

        // First call populates cache
        await query.ExecuteAsync();

        // Remove all episodes from database
        _dbContext.Episodes.RemoveRange(_dbContext.Episodes);
        await _dbContext.SaveChangesAsync();

        // Second call should return cached data
        var result = await query.ExecuteAsync();

        Assert.Equal(3, result.Count); // Should still have 3 from cache
    }

    [Fact]
    public async Task ExecuteAsync_CountsCharactersCorrectly()
    {
        var query = new GetEpisodesQuery(_dbContext, _cache);

        var result = await query.ExecuteAsync();

        var episode1 = result.First(e => e.Id == 1);
        var episode2 = result.First(e => e.Id == 2);
        var episode3 = result.First(e => e.Id == 3);

        Assert.Equal(2, episode1.CharacterCount);
        Assert.Equal(1, episode2.CharacterCount);
        Assert.Equal(0, episode3.CharacterCount);
    }

    private async Task SeedTestDataAsync()
    {
        var episodes = new List<Episode>
        {
            new()
            {
                Id = 1,
                Name = "Pilot",
                EpisodeCode = "S01E01",
                AirDate = "December 2, 2013",
                Url = "https://rickandmortyapi.com/api/episode/1",
                Characters = new List<string>
                {
                    "https://rickandmortyapi.com/api/character/1",
                    "https://rickandmortyapi.com/api/character/2"
                },
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                Name = "Lawnmower Dog",
                EpisodeCode = "S01E02",
                AirDate = "December 9, 2013",
                Url = "https://rickandmortyapi.com/api/episode/2",
                Characters = new List<string>
                {
                    "https://rickandmortyapi.com/api/character/1"
                },
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 3,
                Name = "Anatomy Park",
                EpisodeCode = "S01E03",
                AirDate = "December 16, 2013",
                Url = "https://rickandmortyapi.com/api/episode/3",
                Characters = new List<string>(),
                CreatedAt = DateTime.UtcNow
            }
        };

        await _dbContext.Episodes.AddRangeAsync(episodes);
        await _dbContext.SaveChangesAsync();
    }
}
