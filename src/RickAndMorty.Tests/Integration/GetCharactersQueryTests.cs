using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RickAndMorty.Domain.Models;
using RickAndMorty.Infrastructure;
using RickAndMorty.Web.Features.GetCharacters;

namespace RickAndMorty.Tests.Integration;

public sealed class GetCharactersQueryTests : IAsyncLifetime
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
    public async Task ExecuteAsync_ReturnsAllCharacters_WhenCacheIsEmpty()
    {
        var query = new GetCharactersQuery(_dbContext, _cache);

        var result = await query.ExecuteAsync();

        Assert.NotNull(result);
        Assert.Equal(5, result.Characters.Count);
        Assert.True(result.FromDatabase);
        Assert.NotNull(result.LastFetchedAt);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCharactersInAlphabeticalOrder()
    {
        var query = new GetCharactersQuery(_dbContext, _cache);

        var result = await query.ExecuteAsync();

        var orderedNames = result.Characters.Select(c => c.Name).ToList();
        var expectedOrder = orderedNames.OrderBy(n => n).ToList();
        
        Assert.Equal(expectedOrder, orderedNames);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCachedData_WhenCalledWithinFiveMinutes()
    {
        var query = new GetCharactersQuery(_dbContext, _cache);

        var firstResult = await query.ExecuteAsync();
        var secondResult = await query.ExecuteAsync();

        Assert.Equal(firstResult.Characters.Count, secondResult.Characters.Count);
        Assert.True(secondResult.FromDatabase);
        Assert.Equal(firstResult.LastFetchedAt, secondResult.LastFetchedAt);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOnlyAliveCharacters()
    {
        var query = new GetCharactersQuery(_dbContext, _cache);

        var result = await query.ExecuteAsync();

        Assert.All(result.Characters, character => 
            Assert.Equal("Alive", character.Status)
        );
    }

    [Fact]
    public async Task ExecuteAsync_MapsCharacterPropertiesCorrectly()
    {
        var query = new GetCharactersQuery(_dbContext, _cache);

        var result = await query.ExecuteAsync();

        var firstCharacter = result.Characters.First();
        Assert.NotEqual(0, firstCharacter.Id);
        Assert.NotEmpty(firstCharacter.Name);
        Assert.NotEmpty(firstCharacter.Species);
        Assert.NotEmpty(firstCharacter.OriginName);
    }

    private async Task SeedTestDataAsync()
    {
        var characters = new List<Character>
        {
            new()
            {
                Id = 1,
                Name = "Rick Sanchez",
                Status = "Alive",
                Species = "Human",
                Origin = new Location { Name = "Earth (C-137)", Url = "" },
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                Name = "Morty Smith",
                Status = "Alive",
                Species = "Human",
                Origin = new Location { Name = "Earth (C-137)", Url = "" },
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 3,
                Name = "Summer Smith",
                Status = "Alive",
                Species = "Human",
                Origin = new Location { Name = "Earth (Replacement Dimension)", Url = "" },
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 4,
                Name = "Beth Smith",
                Status = "Alive",
                Species = "Human",
                Origin = new Location { Name = "Earth (Replacement Dimension)", Url = "" },
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 5,
                Name = "Jerry Smith",
                Status = "Alive",
                Species = "Human",
                Origin = new Location { Name = "Earth (Replacement Dimension)", Url = "" },
                CreatedAt = DateTime.UtcNow
            }
        };

        await _dbContext.Characters.AddRangeAsync(characters);
        await _dbContext.SaveChangesAsync();
    }
}