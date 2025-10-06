using RickAndMorty.Domain.Models;

namespace RickAndMorty.Tests.Unit;

public sealed class CharacterTests
{
    [Fact]
    public void Character_CanBeCreated_WithAllProperties()
    {
        var createdAt = DateTime.UtcNow;
        var location = new Location { Name = "Earth", Url = "https://example.com/earth" };

        var character = new Character
        {
            Id = 1,
            Name = "Rick Sanchez",
            Status = "Alive",
            Species = "Human",
            ImageUrl = "https://example.com/rick.jpg",
            Origin = location,
            CreatedAt = createdAt
        };

        Assert.Equal(1, character.Id);
        Assert.Equal("Rick Sanchez", character.Name);
        Assert.Equal("Alive", character.Status);
        Assert.Equal("Human", character.Species);
        Assert.Equal("https://example.com/rick.jpg", character.ImageUrl);
        Assert.Equal(location, character.Origin);
        Assert.Equal(createdAt, character.CreatedAt);
    }

    [Fact]
    public void Character_CanHaveDifferentOrigins()
    {
        var origin1 = new Location { Name = "Earth (C-137)", Url = "earth.com" };
        var origin2 = new Location { Name = "Planet X", Url = "planetx.com" };

        var character1 = new Character
        {
            Id = 1,
            Name = "Rick",
            Status = "Alive",
            Species = "Human",
            ImageUrl = "rick.jpg",
            Origin = origin1,
            CreatedAt = DateTime.UtcNow
        };

        var character2 = new Character
        {
            Id = 2,
            Name = "Alien",
            Status = "Alive",
            Species = "Alien",
            ImageUrl = "alien.jpg",
            Origin = origin2,
            CreatedAt = DateTime.UtcNow
        };

        Assert.NotEqual(character1.Origin.Name, character2.Origin.Name);
    }
}
