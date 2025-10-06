using Microsoft.EntityFrameworkCore;
using RickAndMorty.Domain.Models;
using RickAndMorty.Infrastructure;

namespace RickAndMorty.Tests.Integration;

public sealed class AppDbContextTests : IAsyncLifetime
{
    private AppDbContext _dbContext = null!;

    public Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new AppDbContext(options);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _dbContext.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Characters_CanAddAndRetrieve()
    {
        var character = new Character
        {
            Id = 1,
            Name = "Test Character",
            Status = "Alive",
            Species = "Human",
            ImageUrl = "https://example.com/test.jpg",
            Origin = new Location { Name = "Earth", Url = "https://example.com/earth" },
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Characters.AddAsync(character);
        await _dbContext.SaveChangesAsync();

        var retrieved = await _dbContext.Characters.FindAsync(1);

        Assert.NotNull(retrieved);
        Assert.Equal("Test Character", retrieved.Name);
        Assert.Equal("Alive", retrieved.Status);
        Assert.Equal("Human", retrieved.Species);
        Assert.Equal("Earth", retrieved.Origin.Name);
    }

    [Fact]
    public async Task Characters_OwnedLocation_IsPersisted()
    {
        var character = new Character
        {
            Id = 1,
            Name = "Test",
            Status = "Alive",
            Species = "Human",
            ImageUrl = "https://example.com/test.jpg",
            Origin = new Location
            {
                Name = "Earth (C-137)",
                Url = "https://example.com/earth"
            },
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Characters.AddAsync(character);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        var retrieved = await _dbContext.Characters.FindAsync(1);

        Assert.NotNull(retrieved);
        Assert.NotNull(retrieved.Origin);
        Assert.Equal("Earth (C-137)", retrieved.Origin.Name);
        Assert.Equal("https://example.com/earth", retrieved.Origin.Url);
    }

    [Fact]
    public async Task Characters_CanAddWithRequiredFields()
    {
        var character = new Character
        {
            Id = 100,
            Name = "Test Character",
            Status = "Alive",
            Species = "Human",
            ImageUrl = "https://example.com/test.jpg",
            Origin = new Location { Name = "Earth", Url = "" },
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Characters.AddAsync(character);
        await _dbContext.SaveChangesAsync();

        var retrieved = await _dbContext.Characters.FindAsync(100);
        Assert.NotNull(retrieved);
        Assert.Equal("Test Character", retrieved.Name);
    }

    [Fact]
    public async Task Episodes_CanAddAndRetrieve()
    {
        var episode = new Episode
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
        };

        await _dbContext.Episodes.AddAsync(episode);
        await _dbContext.SaveChangesAsync();

        var retrieved = await _dbContext.Episodes.FindAsync(1);

        Assert.NotNull(retrieved);
        Assert.Equal("Pilot", retrieved.Name);
        Assert.Equal("S01E01", retrieved.EpisodeCode);
        Assert.Equal(2, retrieved.Characters.Count);
    }

    [Fact]
    public async Task Episodes_CharactersList_IsPersistedCorrectly()
    {
        var episode = new Episode
        {
            Id = 1,
            Name = "Test Episode",
            EpisodeCode = "S01E01",
            AirDate = "December 2, 2013",
            Url = "https://rickandmortyapi.com/api/episode/1",
            Characters = new List<string>
            {
                "https://rickandmortyapi.com/api/character/1",
                "https://rickandmortyapi.com/api/character/2",
                "https://rickandmortyapi.com/api/character/3"
            },
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Episodes.AddAsync(episode);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        var retrieved = await _dbContext.Episodes.FindAsync(1);

        Assert.NotNull(retrieved);
        Assert.Equal(3, retrieved.Characters.Count);
        Assert.Contains("https://rickandmortyapi.com/api/character/1", retrieved.Characters);
        Assert.Contains("https://rickandmortyapi.com/api/character/2", retrieved.Characters);
        Assert.Contains("https://rickandmortyapi.com/api/character/3", retrieved.Characters);
    }

    [Fact]
    public async Task Episodes_EmptyCharactersList_IsPersistedCorrectly()
    {
        var episode = new Episode
        {
            Id = 1,
            Name = "Test Episode",
            EpisodeCode = "S01E01",
            AirDate = "December 2, 2013",
            Url = "https://rickandmortyapi.com/api/episode/1",
            Characters = new List<string>(),
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Episodes.AddAsync(episode);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        var retrieved = await _dbContext.Episodes.FindAsync(1);

        Assert.NotNull(retrieved);
        Assert.Empty(retrieved.Characters);
    }

    [Fact]
    public async Task Characters_CanQueryByStatus()
    {
        var characters = new[]
        {
            new Character
            {
                Id = 1,
                Name = "Alive Character",
                Status = "Alive",
                Species = "Human",
                ImageUrl = "https://example.com/alive.jpg",
                Origin = new Location { Name = "Earth", Url = "" },
                CreatedAt = DateTime.UtcNow
            },
            new Character
            {
                Id = 2,
                Name = "Dead Character",
                Status = "Dead",
                Species = "Human",
                ImageUrl = "https://example.com/dead.jpg",
                Origin = new Location { Name = "Earth", Url = "" },
                CreatedAt = DateTime.UtcNow
            }
        };

        await _dbContext.Characters.AddRangeAsync(characters);
        await _dbContext.SaveChangesAsync();

        var aliveCharacters = await _dbContext.Characters
            .Where(c => c.Status == "Alive")
            .ToListAsync();

        Assert.Single(aliveCharacters);
        Assert.Equal("Alive Character", aliveCharacters[0].Name);
    }

    [Fact]
    public async Task Episodes_CanQueryByEpisodeCode()
    {
        var episodes = new[]
        {
            new Episode
            {
                Id = 1,
                Name = "Pilot",
                EpisodeCode = "S01E01",
                AirDate = "December 2, 2013",
                Url = "https://rickandmortyapi.com/api/episode/1",
                Characters = new List<string>(),
                CreatedAt = DateTime.UtcNow
            },
            new Episode
            {
                Id = 2,
                Name = "Second Episode",
                EpisodeCode = "S01E02",
                AirDate = "December 9, 2013",
                Url = "https://rickandmortyapi.com/api/episode/2",
                Characters = new List<string>(),
                CreatedAt = DateTime.UtcNow
            }
        };

        await _dbContext.Episodes.AddRangeAsync(episodes);
        await _dbContext.SaveChangesAsync();

        var episode = await _dbContext.Episodes
            .Where(e => e.EpisodeCode == "S01E01")
            .FirstOrDefaultAsync();

        Assert.NotNull(episode);
        Assert.Equal("Pilot", episode.Name);
    }

    [Fact]
    public async Task Characters_CanUpdate()
    {
        var character = new Character
        {
            Id = 1,
            Name = "Original Name",
            Status = "Alive",
            Species = "Human",
            ImageUrl = "https://example.com/test.jpg",
            Origin = new Location { Name = "Earth", Url = "" },
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Characters.AddAsync(character);
        await _dbContext.SaveChangesAsync();

        // Clear tracking before update
        _dbContext.ChangeTracker.Clear();

        // Create updated character instance
        var updatedCharacter = new Character
        {
            Id = 1,
            Name = "Updated Name",
            Status = "Dead",
            Species = character.Species,
            ImageUrl = character.ImageUrl,
            Origin = character.Origin,
            CreatedAt = character.CreatedAt
        };

        _dbContext.Characters.Update(updatedCharacter);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        var retrieved = await _dbContext.Characters.FindAsync(1);

        Assert.NotNull(retrieved);
        Assert.Equal("Updated Name", retrieved.Name);
        Assert.Equal("Dead", retrieved.Status);
    }

    [Fact]
    public async Task Characters_CanDelete()
    {
        var character = new Character
        {
            Id = 1,
            Name = "To Delete",
            Status = "Alive",
            Species = "Human",
            ImageUrl = "https://example.com/test.jpg",
            Origin = new Location { Name = "Earth", Url = "" },
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Characters.AddAsync(character);
        await _dbContext.SaveChangesAsync();

        _dbContext.Characters.Remove(character);
        await _dbContext.SaveChangesAsync();

        var retrieved = await _dbContext.Characters.FindAsync(1);

        Assert.Null(retrieved);
    }

    [Fact]
    public async Task Characters_CreatedAt_IsPersisted()
    {
        var createdAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var character = new Character
        {
            Id = 1,
            Name = "Test",
            Status = "Alive",
            Species = "Human",
            ImageUrl = "https://example.com/test.jpg",
            Origin = new Location { Name = "Earth", Url = "" },
            CreatedAt = createdAt
        };

        await _dbContext.Characters.AddAsync(character);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        var retrieved = await _dbContext.Characters.FindAsync(1);

        Assert.NotNull(retrieved);
        Assert.Equal(createdAt, retrieved.CreatedAt);
    }
}
