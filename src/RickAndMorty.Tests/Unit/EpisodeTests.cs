using RickAndMorty.Domain.Models;

namespace RickAndMorty.Tests.Unit;

public sealed class EpisodeTests
{
    [Fact]
    public void Episode_CanBeCreated_WithAllProperties()
    {
        var characters = new List<string>
        {
            "https://rickandmortyapi.com/api/character/1",
            "https://rickandmortyapi.com/api/character/2"
        };
        var createdAt = DateTime.UtcNow;

        var episode = new Episode
        {
            Id = 1,
            Name = "Pilot",
            EpisodeCode = "S01E01",
            AirDate = "December 2, 2013",
            Url = "https://rickandmortyapi.com/api/episode/1",
            Characters = characters,
            CreatedAt = createdAt
        };

        Assert.Equal(1, episode.Id);
        Assert.Equal("Pilot", episode.Name);
        Assert.Equal("S01E01", episode.EpisodeCode);
        Assert.Equal("December 2, 2013", episode.AirDate);
        Assert.Equal("https://rickandmortyapi.com/api/episode/1", episode.Url);
        Assert.Equal(2, episode.Characters.Count);
        Assert.Equal(createdAt, episode.CreatedAt);
    }

    [Fact]
    public void Episode_Characters_CanBeModified()
    {
        var episode = new Episode
        {
            Id = 1,
            Name = "Test",
            EpisodeCode = "S01E01",
            AirDate = "Test Date",
            Url = "test.com",
            Characters = new List<string> { "character1" },
            CreatedAt = DateTime.UtcNow
        };

        episode.Characters.Add("character2");
        episode.Characters.Add("character3");

        Assert.Equal(3, episode.Characters.Count);
        Assert.Contains("character1", episode.Characters);
        Assert.Contains("character2", episode.Characters);
        Assert.Contains("character3", episode.Characters);
    }
}
