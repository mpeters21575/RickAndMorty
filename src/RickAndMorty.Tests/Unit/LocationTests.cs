using RickAndMorty.Domain.Models;

namespace RickAndMorty.Tests.Unit;

public sealed class LocationTests
{
    [Fact]
    public void Location_CanBeCreated()
    {
        var location = new Location
        {
            Name = "Earth (C-137)",
            Url = "https://rickandmortyapi.com/api/location/1"
        };

        Assert.Equal("Earth (C-137)", location.Name);
        Assert.Equal("https://rickandmortyapi.com/api/location/1", location.Url);
    }

    [Fact]
    public void Location_CanHaveEmptyUrl()
    {
        var location = new Location
        {
            Name = "Unknown",
            Url = string.Empty
        };

        Assert.Equal("Unknown", location.Name);
        Assert.Equal(string.Empty, location.Url);
    }

    [Fact]
    public void Location_CanBeCreated_WithDifferentValues()
    {
        var location1 = new Location { Name = "Earth", Url = "earth.com" };
        var location2 = new Location { Name = "Mars", Url = "mars.com" };

        Assert.NotEqual(location1.Name, location2.Name);
        Assert.NotEqual(location1.Url, location2.Url);
    }
}
