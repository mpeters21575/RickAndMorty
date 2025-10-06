using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using RickAndMorty.Console.Features;
using RickAndMorty.Domain.Models;
using RickAndMorty.Infrastructure;
using RickAndMorty.Infrastructure.Configuration;

namespace RickAndMorty.Tests.Unit;

public sealed class FetchEpisodesServiceTests : IAsyncLifetime
{
    private AppDbContext _dbContext = null!;
    private HttpClient _httpClient = null!;
    private Mock<HttpMessageHandler> _httpMessageHandler = null!;
    private IOptions<RickAndMortyApiSettings> _apiSettings = null!;

    public Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new AppDbContext(options);

        _httpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandler.Object);

        _apiSettings = Options.Create(new RickAndMortyApiSettings
        {
            BaseUrl = "https://rickandmortyapi.com/api",
            CharacterEndpoint = "/character"
        });

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _dbContext.Dispose();
        _httpClient.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_InsertsNewEpisodes()
    {
        var apiResponse = """
        {
            "info": { "pages": 1 },
            "results": [
                {
                    "id": 1,
                    "name": "Pilot",
                    "air_date": "December 2, 2013",
                    "episode": "S01E01",
                    "characters": [
                        "https://rickandmortyapi.com/api/character/1",
                        "https://rickandmortyapi.com/api/character/2"
                    ],
                    "url": "https://rickandmortyapi.com/api/episode/1"
                },
                {
                    "id": 2,
                    "name": "Lawnmower Dog",
                    "air_date": "December 9, 2013",
                    "episode": "S01E02",
                    "characters": [
                        "https://rickandmortyapi.com/api/character/1"
                    ],
                    "url": "https://rickandmortyapi.com/api/episode/2"
                }
            ]
        }
        """;

        SetupHttpResponse(apiResponse);

        var service = new FetchEpisodesService(_httpClient, _dbContext, _apiSettings);
        await service.ExecuteAsync();

        var savedEpisodes = await _dbContext.Episodes.ToListAsync();

        Assert.Equal(2, savedEpisodes.Count);
        Assert.Contains(savedEpisodes, e => e.Name == "Pilot");
        Assert.Contains(savedEpisodes, e => e.Name == "Lawnmower Dog");
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesExistingEpisodes()
    {
        await SeedExistingEpisodeAsync();

        // Clear change tracker to avoid tracking conflicts
        _dbContext.ChangeTracker.Clear();

        var apiResponse = """
        {
            "info": { "pages": 1 },
            "results": [
                {
                    "id": 1,
                    "name": "Pilot - Updated",
                    "air_date": "December 2, 2013",
                    "episode": "S01E01",
                    "characters": [
                        "https://rickandmortyapi.com/api/character/1"
                    ],
                    "url": "https://rickandmortyapi.com/api/episode/1"
                }
            ]
        }
        """;

        SetupHttpResponse(apiResponse);

        var service = new FetchEpisodesService(_httpClient, _dbContext, _apiSettings);
        await service.ExecuteAsync();

        _dbContext.ChangeTracker.Clear();
        var updatedEpisode = await _dbContext.Episodes.FindAsync(1);

        Assert.NotNull(updatedEpisode);
        Assert.Equal("Pilot - Updated", updatedEpisode.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesMultiplePages()
    {
        var page1Response = """
        {
            "info": { "pages": 2 },
            "results": [
                {
                    "id": 1,
                    "name": "Episode 1",
                    "air_date": "December 2, 2013",
                    "episode": "S01E01",
                    "characters": [],
                    "url": "https://rickandmortyapi.com/api/episode/1"
                }
            ]
        }
        """;

        var page2Response = """
        {
            "info": { "pages": 2 },
            "results": [
                {
                    "id": 2,
                    "name": "Episode 2",
                    "air_date": "December 9, 2013",
                    "episode": "S01E02",
                    "characters": [],
                    "url": "https://rickandmortyapi.com/api/episode/2"
                }
            ]
        }
        """;

        _httpMessageHandler
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(page1Response)
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(page2Response)
            });

        var service = new FetchEpisodesService(_httpClient, _dbContext, _apiSettings);
        await service.ExecuteAsync();

        var savedEpisodes = await _dbContext.Episodes.ToListAsync();

        Assert.Equal(2, savedEpisodes.Count);
        Assert.Contains(savedEpisodes, e => e.Name == "Episode 1");
        Assert.Contains(savedEpisodes, e => e.Name == "Episode 2");
    }

    [Fact]
    public async Task ExecuteAsync_PreservesApiIdAsEpisodeId()
    {
        var apiResponse = """
        {
            "info": { "pages": 1 },
            "results": [
                {
                    "id": 999,
                    "name": "Test Episode",
                    "air_date": "December 2, 2013",
                    "episode": "S99E99",
                    "characters": [],
                    "url": "https://rickandmortyapi.com/api/episode/999"
                }
            ]
        }
        """;

        SetupHttpResponse(apiResponse);

        var service = new FetchEpisodesService(_httpClient, _dbContext, _apiSettings);
        await service.ExecuteAsync();

        var savedEpisode = await _dbContext.Episodes.SingleAsync();

        Assert.Equal(999, savedEpisode.Id);
    }

    [Fact]
    public async Task ExecuteAsync_MapsAllPropertiesCorrectly()
    {
        var apiResponse = """
        {
            "info": { "pages": 1 },
            "results": [
                {
                    "id": 1,
                    "name": "Pilot",
                    "air_date": "December 2, 2013",
                    "episode": "S01E01",
                    "characters": [
                        "https://rickandmortyapi.com/api/character/1",
                        "https://rickandmortyapi.com/api/character/2"
                    ],
                    "url": "https://rickandmortyapi.com/api/episode/1"
                }
            ]
        }
        """;

        SetupHttpResponse(apiResponse);

        var service = new FetchEpisodesService(_httpClient, _dbContext, _apiSettings);
        await service.ExecuteAsync();

        var savedEpisode = await _dbContext.Episodes.SingleAsync();

        Assert.Equal(1, savedEpisode.Id);
        Assert.Equal("Pilot", savedEpisode.Name);
        Assert.Equal("December 2, 2013", savedEpisode.AirDate);
        Assert.Equal("S01E01", savedEpisode.EpisodeCode);
        Assert.Equal("https://rickandmortyapi.com/api/episode/1", savedEpisode.Url);
        Assert.Equal(2, savedEpisode.Characters.Count);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesEmptyCharactersList()
    {
        var apiResponse = """
        {
            "info": { "pages": 1 },
            "results": [
                {
                    "id": 1,
                    "name": "Empty Episode",
                    "air_date": "December 2, 2013",
                    "episode": "S01E01",
                    "characters": [],
                    "url": "https://rickandmortyapi.com/api/episode/1"
                }
            ]
        }
        """;

        SetupHttpResponse(apiResponse);

        var service = new FetchEpisodesService(_httpClient, _dbContext, _apiSettings);
        await service.ExecuteAsync();

        var savedEpisode = await _dbContext.Episodes.SingleAsync();

        Assert.Empty(savedEpisode.Characters);
    }

    [Fact]
    public async Task ExecuteAsync_MixesNewAndExistingEpisodes()
    {
        await SeedExistingEpisodeAsync();

        // Clear change tracker to avoid tracking conflicts
        _dbContext.ChangeTracker.Clear();

        var apiResponse = """
        {
            "info": { "pages": 1 },
            "results": [
                {
                    "id": 1,
                    "name": "Pilot - Updated",
                    "air_date": "December 2, 2013",
                    "episode": "S01E01",
                    "characters": [],
                    "url": "https://rickandmortyapi.com/api/episode/1"
                },
                {
                    "id": 2,
                    "name": "New Episode",
                    "air_date": "December 9, 2013",
                    "episode": "S01E02",
                    "characters": [],
                    "url": "https://rickandmortyapi.com/api/episode/2"
                }
            ]
        }
        """;

        SetupHttpResponse(apiResponse);

        var service = new FetchEpisodesService(_httpClient, _dbContext, _apiSettings);
        await service.ExecuteAsync();

        _dbContext.ChangeTracker.Clear();
        var savedEpisodes = await _dbContext.Episodes.ToListAsync();

        Assert.Equal(2, savedEpisodes.Count);
        Assert.Contains(savedEpisodes, e => e.Id == 1 && e.Name == "Pilot - Updated");
        Assert.Contains(savedEpisodes, e => e.Id == 2 && e.Name == "New Episode");
    }

    private void SetupHttpResponse(string responseContent)
    {
        _httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });
    }

    private async Task SeedExistingEpisodeAsync()
    {
        var existingEpisode = new Episode
        {
            Id = 1,
            Name = "Pilot",
            AirDate = "December 2, 2013",
            EpisodeCode = "S01E01",
            Url = "https://rickandmortyapi.com/api/episode/1",
            Characters = new List<string>(),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        await _dbContext.Episodes.AddAsync(existingEpisode);
        await _dbContext.SaveChangesAsync();
    }
}
