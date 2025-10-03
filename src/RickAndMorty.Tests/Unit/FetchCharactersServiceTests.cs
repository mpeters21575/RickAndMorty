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

public sealed class FetchCharactersServiceTests : IAsyncLifetime
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
    public async Task ExecuteAsync_SavesOnlyAliveCharacters()
    {
        var apiResponse = """
        {
            "info": { "pages": 1 },
            "results": [
                {
                    "id": 1,
                    "name": "Rick",
                    "status": "Alive",
                    "species": "Human",
                    "origin": { "name": "Earth", "url": "" }
                },
                {
                    "id": 2,
                    "name": "Dead Character",
                    "status": "Dead",
                    "species": "Human",
                    "origin": { "name": "Earth", "url": "" }
                },
                {
                    "id": 3,
                    "name": "Morty",
                    "status": "Alive",
                    "species": "Human",
                    "origin": { "name": "Earth", "url": "" }
                }
            ]
        }
        """;

        SetupHttpResponse(apiResponse);

        var service = new FetchCharactersService(_httpClient, _dbContext, _apiSettings);
        await service.ExecuteAsync();

        var savedCharacters = await _dbContext.Characters.ToListAsync();
        
        Assert.Equal(2, savedCharacters.Count);
        Assert.All(savedCharacters, c => Assert.Equal("Alive", c.Status));
    }

    [Fact]
    public async Task ExecuteAsync_AddsOnlyNewCharacters()
    {
        await SeedExistingCharactersAsync();
        
        var apiResponse = """
        {
            "info": { "pages": 1 },
            "results": [
                {
                    "id": 100,
                    "name": "New Character",
                    "status": "Alive",
                    "species": "Alien",
                    "origin": { "name": "Unknown", "url": "" }
                }
            ]
        }
        """;

        SetupHttpResponse(apiResponse);

        var service = new FetchCharactersService(_httpClient, _dbContext, _apiSettings);
        await service.ExecuteAsync();

        var savedCharacters = await _dbContext.Characters.ToListAsync();
        
        Assert.Equal(2, savedCharacters.Count);
        Assert.Contains(savedCharacters, c => c.Id == 50);
        Assert.Contains(savedCharacters, c => c.Id == 100);
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
                    "name": "Character 1",
                    "status": "Alive",
                    "species": "Human",
                    "origin": { "name": "Earth", "url": "" }
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
                    "name": "Character 2",
                    "status": "Alive",
                    "species": "Alien",
                    "origin": { "name": "Mars", "url": "" }
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

        var service = new FetchCharactersService(_httpClient, _dbContext, _apiSettings);
        await service.ExecuteAsync();

        var savedCharacters = await _dbContext.Characters.ToListAsync();
        
        Assert.Equal(2, savedCharacters.Count);
        Assert.Contains(savedCharacters, c => c.Name == "Character 1");
        Assert.Contains(savedCharacters, c => c.Name == "Character 2");
    }

    [Fact]
    public async Task ExecuteAsync_UsesApiSettingsFromConfiguration()
    {
        var apiResponse = """
        {
            "info": { "pages": 1 },
            "results": [
                {
                    "id": 1,
                    "name": "Test Character",
                    "status": "Alive",
                    "species": "Human",
                    "origin": { "name": "Earth", "url": "" }
                }
            ]
        }
        """;

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(apiResponse)
            });

        var service = new FetchCharactersService(_httpClient, _dbContext, _apiSettings);
        await service.ExecuteAsync();

        Assert.NotNull(capturedRequest);
        Assert.Equal("https://rickandmortyapi.com/api/character?page=1", capturedRequest!.RequestUri!.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_PreservesApiIdAsCharacterId()
    {
        var apiResponse = """
        {
            "info": { "pages": 1 },
            "results": [
                {
                    "id": 999,
                    "name": "Test Character",
                    "status": "Alive",
                    "species": "Human",
                    "origin": { "name": "Earth", "url": "" }
                }
            ]
        }
        """;

        SetupHttpResponse(apiResponse);

        var service = new FetchCharactersService(_httpClient, _dbContext, _apiSettings);
        await service.ExecuteAsync();

        var savedCharacter = await _dbContext.Characters.SingleAsync();
        
        Assert.Equal(999, savedCharacter.Id);
    }

    [Fact]
    public async Task ExecuteAsync_FiltersOutDeadAndUnknownCharacters()
    {
        var apiResponse = """
        {
            "info": { "pages": 1 },
            "results": [
                {
                    "id": 1,
                    "name": "Alive Character",
                    "status": "Alive",
                    "species": "Human",
                    "origin": { "name": "Earth", "url": "" }
                },
                {
                    "id": 2,
                    "name": "Dead Character",
                    "status": "Dead",
                    "species": "Human",
                    "origin": { "name": "Earth", "url": "" }
                },
                {
                    "id": 3,
                    "name": "Unknown Character",
                    "status": "unknown",
                    "species": "Human",
                    "origin": { "name": "Earth", "url": "" }
                }
            ]
        }
        """;

        SetupHttpResponse(apiResponse);

        var service = new FetchCharactersService(_httpClient, _dbContext, _apiSettings);
        await service.ExecuteAsync();

        var savedCharacters = await _dbContext.Characters.ToListAsync();
        
        Assert.Single(savedCharacters);
        Assert.Equal("Alive Character", savedCharacters[0].Name);
        Assert.Equal("Alive", savedCharacters[0].Status);
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

    private async Task SeedExistingCharactersAsync()
    {
        var existingCharacters = new[]
        {
            new Character
            {
                Id = 50,
                Name = "Old Character",
                Status = "Alive",
                Species = "Human",
                Origin = new Location { Name = "Earth", Url = "" },
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        await _dbContext.Characters.AddRangeAsync(existingCharacters);
        await _dbContext.SaveChangesAsync();
    }
}