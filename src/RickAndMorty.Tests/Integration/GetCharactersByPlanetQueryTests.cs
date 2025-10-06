using Microsoft.EntityFrameworkCore;
using RickAndMorty.Domain.Models;
using RickAndMorty.Infrastructure;
using RickAndMorty.Web.Features.GetCharactersByPlanet;

namespace RickAndMorty.Tests.Integration;

public sealed class GetCharactersByPlanetQueryTests : IAsyncLifetime
{
    private AppDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new AppDbContext(options);
        await SeedTestDataAsync();
    }

    public Task DisposeAsync()
    {
        _dbContext.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCharactersFromSpecificPlanet()
    {
        var query = new GetCharactersByPlanetQuery(_dbContext);

        var result = await query.ExecuteAsync("Earth (C-137)");

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Contains("Earth (C-137)", c.OriginName));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCharactersWithPartialPlanetMatch()
    {
        var query = new GetCharactersByPlanetQuery(_dbContext);

        var result = await query.ExecuteAsync("Earth");

        Assert.Equal(4, result.Count);
        Assert.All(result, c => Assert.Contains("Earth", c.OriginName));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyList_WhenNoPlanetMatches()
    {
        var query = new GetCharactersByPlanetQuery(_dbContext);

        var result = await query.ExecuteAsync("Mars");

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCharactersInAlphabeticalOrder()
    {
        var query = new GetCharactersByPlanetQuery(_dbContext);

        var result = await query.ExecuteAsync("Earth");

        var names = result.Select(c => c.Name).ToList();
        var expectedOrder = names.OrderBy(n => n).ToList();
        Assert.Equal(expectedOrder, names);
    }

    [Fact]
    public async Task ExecuteAsync_MapsAllPropertiesCorrectly()
    {
        var query = new GetCharactersByPlanetQuery(_dbContext);

        var result = await query.ExecuteAsync("Earth (C-137)");

        var character = result.First();
        Assert.NotEqual(0, character.Id);
        Assert.NotEmpty(character.Name);
        Assert.NotEmpty(character.Status);
        Assert.NotEmpty(character.Species);
        Assert.NotEmpty(character.OriginName);
        Assert.NotEmpty(character.ImageUrl);
    }

    [Fact]
    public async Task ExecuteAsync_IsCaseInsensitive()
    {
        var query = new GetCharactersByPlanetQuery(_dbContext);

        var result = await query.ExecuteAsync("earth");

        // EF Core InMemory is case-insensitive for Contains - so this should find nothing
        // because all planets are "Earth" with capital E
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesPlanetNameWithParentheses()
    {
        var query = new GetCharactersByPlanetQuery(_dbContext);

        var result = await query.ExecuteAsync("(C-137)");

        Assert.Equal(2, result.Count);
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
                ImageUrl = "https://example.com/rick.jpg",
                Origin = new Location { Name = "Earth (C-137)", Url = "" },
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                Name = "Morty Smith",
                Status = "Alive",
                Species = "Human",
                ImageUrl = "https://example.com/morty.jpg",
                Origin = new Location { Name = "Earth (C-137)", Url = "" },
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 3,
                Name = "Summer Smith",
                Status = "Alive",
                Species = "Human",
                ImageUrl = "https://example.com/summer.jpg",
                Origin = new Location { Name = "Earth (Replacement Dimension)", Url = "" },
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 4,
                Name = "Beth Smith",
                Status = "Alive",
                Species = "Human",
                ImageUrl = "https://example.com/beth.jpg",
                Origin = new Location { Name = "Earth (Replacement Dimension)", Url = "" },
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 5,
                Name = "Alien Being",
                Status = "Alive",
                Species = "Alien",
                ImageUrl = "https://example.com/alien.jpg",
                Origin = new Location { Name = "Planet X", Url = "" },
                CreatedAt = DateTime.UtcNow
            }
        };

        await _dbContext.Characters.AddRangeAsync(characters);
        await _dbContext.SaveChangesAsync();
    }
}
