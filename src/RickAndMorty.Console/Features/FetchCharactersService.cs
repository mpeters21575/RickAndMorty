using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RickAndMorty.Console.Models;
using RickAndMorty.Domain.Models;
using RickAndMorty.Infrastructure;
using RickAndMorty.Infrastructure.Configuration;

namespace RickAndMorty.Console.Features;

public interface IFetchCharactersService
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}

public sealed class FetchCharactersService(
    HttpClient httpClient,
    AppDbContext dbContext,
    IOptions<RickAndMortyApiSettings> apiSettings)
    : IFetchCharactersService
{
    private readonly RickAndMortyApiSettings _apiSettings = apiSettings.Value;
    private readonly JsonSerializerOptions _jsonOptions = new()
    { 
        PropertyNameCaseInsensitive = true 
    };

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var allCharacters = await FetchAllCharactersFromApiAsync(cancellationToken);
        var aliveCharacters = allCharacters
            .Where(c => c.Status.Equals("Alive", StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        var (inserted, updated) = await UpsertCharactersToDatabaseAsync(aliveCharacters, cancellationToken);
        
        System.Console.WriteLine($"Successfully processed {aliveCharacters.Count} alive characters:");
        System.Console.WriteLine($"  - {inserted} new characters inserted");
        System.Console.WriteLine($"  - {updated} existing characters updated");
    }

    private async Task<List<ApiCharacter>> FetchAllCharactersFromApiAsync(CancellationToken cancellationToken)
    {
        var characters = new List<ApiCharacter>();
        var page = 1;
        var totalPages = 1;

        while (page <= totalPages)
        {
            var url = _apiSettings.GetCharacterUrl(page);
            var response = await httpClient.GetAsync(url, cancellationToken);
            
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<ApiResponse>(content, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize API response");
            
            characters.AddRange(apiResponse.Results);
            totalPages = apiResponse.Info.Pages;
            page++;
            
            System.Console.WriteLine($"Fetched page {page - 1}/{totalPages}");
        }

        return characters;
    }

    private async Task<(int inserted, int updated)> UpsertCharactersToDatabaseAsync(
        List<ApiCharacter> apiCharacters, 
        CancellationToken cancellationToken)
    {
        var existingIds = (await dbContext.Characters
            .Select(c => c.Id)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var charactersToAdd = new List<Character>();
        var charactersToUpdate = new List<Character>();

        foreach (var apiCharacter in apiCharacters)
        {
            var character = MapToDomain(apiCharacter);
            
            if (existingIds.Contains(character.Id))
            {
                charactersToUpdate.Add(character);
            }
            else
            {
                charactersToAdd.Add(character);
            }
        }

        if (charactersToAdd.Count > 0)
        {
            await dbContext.Characters.AddRangeAsync(charactersToAdd, cancellationToken);
        }

        if (charactersToUpdate.Count > 0)
        {
            dbContext.Characters.UpdateRange(charactersToUpdate);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return (charactersToAdd.Count, charactersToUpdate.Count);
    }

    private static Character MapToDomain(ApiCharacter apiCharacter) => new()
    {
        Id = apiCharacter.Id,
        Name = apiCharacter.Name,
        Status = apiCharacter.Status,
        Species = apiCharacter.Species,
        Origin = new Location 
        { 
            Name = apiCharacter.Origin.Name, 
            Url = apiCharacter.Origin.Url 
        },
        CreatedAt = DateTime.UtcNow
    };
}