using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RickAndMorty.Domain.Models;
using RickAndMorty.Infrastructure;
using RickAndMorty.Web.Features.AddCharacters;

namespace RickAndMorty.Tests.Integration;

public sealed class AddCharacterCommandTests : IAsyncLifetime
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
    public async Task ExecuteAsync_AddsNewCharacter_ReturnsNewId()
    {
        var command = new AddCharacterCommand(_dbContext, _cache);
        var request = new AddCharacterRequest(
            Name: "New Character",
            Status: "Alive",
            Species: "Alien",
            OriginName: "Planet X"
        );

        var result = await command.ExecuteAsync(request);

        Assert.True(result > 0);
        var savedCharacter = await _dbContext.Characters.FindAsync(result);
        Assert.NotNull(savedCharacter);
        Assert.Equal("New Character", savedCharacter.Name);
        Assert.Equal("Alive", savedCharacter.Status);
        Assert.Equal("Alien", savedCharacter.Species);
        Assert.Equal("Planet X", savedCharacter.Origin.Name);
    }

    [Fact]
    public async Task ExecuteAsync_GeneratesCorrectIncrementalId()
    {
        var command = new AddCharacterCommand(_dbContext, _cache);
        var request = new AddCharacterRequest(
            Name: "Test Character",
            Status: "Alive",
            Species: "Human",
            OriginName: "Earth"
        );

        var result = await command.ExecuteAsync(request);

        var maxExistingId = 3; // From seed data
        Assert.Equal(maxExistingId + 1, result);
    }

    [Fact]
    public async Task ExecuteAsync_SetsCreatedAtToUtcNow()
    {
        var command = new AddCharacterCommand(_dbContext, _cache);
        var beforeExecution = DateTime.UtcNow;

        var request = new AddCharacterRequest(
            Name: "Time Test Character",
            Status: "Alive",
            Species: "Human",
            OriginName: "Earth"
        );

        var result = await command.ExecuteAsync(request);
        var afterExecution = DateTime.UtcNow;

        var savedCharacter = await _dbContext.Characters.FindAsync(result);
        Assert.NotNull(savedCharacter);
        Assert.InRange(savedCharacter.CreatedAt, beforeExecution.AddSeconds(-1), afterExecution.AddSeconds(1));
    }

    [Fact]
    public async Task ExecuteAsync_SetsImageUrlToEmpty()
    {
        var command = new AddCharacterCommand(_dbContext, _cache);
        var request = new AddCharacterRequest(
            Name: "Image Test",
            Status: "Alive",
            Species: "Human",
            OriginName: "Earth"
        );

        var result = await command.ExecuteAsync(request);

        var savedCharacter = await _dbContext.Characters.FindAsync(result);
        Assert.NotNull(savedCharacter);
        Assert.Equal(string.Empty, savedCharacter.ImageUrl);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesEmptyDatabase()
    {
        // Create a fresh database with no characters
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"EmptyDb_{Guid.NewGuid()}")
            .Options;

        using var emptyDbContext = new AppDbContext(options);
        var command = new AddCharacterCommand(emptyDbContext, _cache);

        var request = new AddCharacterRequest(
            Name: "First Character",
            Status: "Alive",
            Species: "Human",
            OriginName: "Earth"
        );

        var result = await command.ExecuteAsync(request);

        Assert.Equal(1, result);
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
            }
        };

        await _dbContext.Characters.AddRangeAsync(characters);
        await _dbContext.SaveChangesAsync();
    }
}
