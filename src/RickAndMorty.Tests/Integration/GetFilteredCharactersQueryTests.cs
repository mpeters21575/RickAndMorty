using Microsoft.EntityFrameworkCore;
using RickAndMorty.Domain.Models;
using RickAndMorty.Infrastructure;
using RickAndMorty.Web.Features.GetFilteredCharacters;

namespace RickAndMorty.Tests.Integration;

public sealed class GetFilteredCharactersQueryTests : IAsyncLifetime
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
    public async Task ExecuteAsync_FiltersCharactersByName()
    {
        var query = new GetFilteredCharactersQuery(_dbContext);

        var result = await query.ExecuteAsync(name: "Rick", null, null);

        Assert.Single(result);
        Assert.Equal("Rick Sanchez", result[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_FiltersCharactersByStatus()
    {
        var query = new GetFilteredCharactersQuery(_dbContext);

        var result = await query.ExecuteAsync(null, status: "Dead", null);

        Assert.Single(result);
        Assert.Equal("Dead Character", result[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_FiltersCharactersBySpecies()
    {
        var query = new GetFilteredCharactersQuery(_dbContext);

        var result = await query.ExecuteAsync(null, null, species: "Alien");

        Assert.Single(result);
        Assert.Equal("Alien Being", result[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_FiltersWithMultipleCriteria()
    {
        var query = new GetFilteredCharactersQuery(_dbContext);

        var result = await query.ExecuteAsync("Smith", "Alive", "Human");

        Assert.Equal(2, result.Count);
        Assert.All(result, c =>
        {
            Assert.Contains("Smith", c.Name);
            Assert.Equal("Alive", c.Status);
            Assert.Equal("Human", c.Species);
        });
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAllCharacters_WhenNoFilters()
    {
        var query = new GetFilteredCharactersQuery(_dbContext);

        var result = await query.ExecuteAsync(null, null, null);

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyList_WhenNoMatches()
    {
        var query = new GetFilteredCharactersQuery(_dbContext);

        var result = await query.ExecuteAsync("NonExistent", null, null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_PerformsCaseInsensitiveSearch()
    {
        var query = new GetFilteredCharactersQuery(_dbContext);

        var result = await query.ExecuteAsync("rick", null, null);

        Assert.Single(result);
        Assert.Equal("Rick Sanchez", result[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_PerformsPartialNameMatch()
    {
        var query = new GetFilteredCharactersQuery(_dbContext);

        var result = await query.ExecuteAsync("Smit", null, null);

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Contains("Smith", c.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCharactersInAlphabeticalOrder()
    {
        var query = new GetFilteredCharactersQuery(_dbContext);

        var result = await query.ExecuteAsync(null, "Alive", null);

        var names = result.Select(c => c.Name).ToList();
        var expectedOrder = names.OrderBy(n => n).ToList();
        Assert.Equal(expectedOrder, names);
    }

    [Fact]
    public async Task ExecuteAsync_MapsAllPropertiesCorrectly()
    {
        var query = new GetFilteredCharactersQuery(_dbContext);

        var result = await query.ExecuteAsync("Rick", null, null);

        var character = result.First();
        Assert.NotEqual(0, character.Id);
        Assert.Equal("Rick Sanchez", character.Name);
        Assert.Equal("Alive", character.Status);
        Assert.Equal("Human", character.Species);
        Assert.Equal("Earth (C-137)", character.OriginName);
        Assert.NotEmpty(character.ImageUrl);
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
                Origin = new Location { Name = "Earth", Url = "" },
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 4,
                Name = "Dead Character",
                Status = "Dead",
                Species = "Human",
                ImageUrl = "https://example.com/dead.jpg",
                Origin = new Location { Name = "Earth", Url = "" },
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
