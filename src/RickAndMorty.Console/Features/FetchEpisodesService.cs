using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RickAndMorty.Console.Models;
using RickAndMorty.Domain.Models;
using RickAndMorty.Infrastructure;
using RickAndMorty.Infrastructure.Configuration;

namespace RickAndMorty.Console.Features;

public interface IFetchEpisodesService
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}

public sealed class FetchEpisodesService(
    HttpClient httpClient,
    AppDbContext dbContext,
    IOptions<RickAndMortyApiSettings> apiSettings)
    : IFetchEpisodesService
{
    private readonly RickAndMortyApiSettings _apiSettings = apiSettings.Value;
    private readonly JsonSerializerOptions _jsonOptions = new()
    { 
        PropertyNameCaseInsensitive = true 
    };

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var allEpisodes = await FetchAllEpisodesFromApiAsync(cancellationToken);
        var (inserted, updated) = await UpsertEpisodesToDatabaseAsync(allEpisodes, cancellationToken);
        
        System.Console.WriteLine($"Successfully processed {allEpisodes.Count} episodes:");
        System.Console.WriteLine($"  - {inserted} new episodes inserted");
        System.Console.WriteLine($"  - {updated} existing episodes updated");
    }

    private async Task<List<ApiEpisode>> FetchAllEpisodesFromApiAsync(CancellationToken cancellationToken)
    {
        var episodes = new List<ApiEpisode>();
        var page = 1;
        var totalPages = 1;

        while (page <= totalPages)
        {
            var url = $"{_apiSettings.BaseUrl.TrimEnd('/')}/episode?page={page}";
            var response = await httpClient.GetAsync(url, cancellationToken);
            
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<ApiEpisodesResponse>(content, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize API response");
            
            episodes.AddRange(apiResponse.Results);
            totalPages = apiResponse.Info.Pages;
            page++;
            
            System.Console.WriteLine($"Fetched page {page - 1}/{totalPages}");
        }

        return episodes;
    }

    private async Task<(int inserted, int updated)> UpsertEpisodesToDatabaseAsync(
        List<ApiEpisode> apiEpisodes, 
        CancellationToken cancellationToken)
    {
        var existingIds = (await dbContext.Episodes
            .Select(e => e.Id)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var episodesToAdd = new List<Episode>();
        var episodesToUpdate = new List<Episode>();

        foreach (var apiEpisode in apiEpisodes)
        {
            var episode = MapToDomain(apiEpisode);
            
            if (existingIds.Contains(episode.Id))
            {
                episodesToUpdate.Add(episode);
            }
            else
            {
                episodesToAdd.Add(episode);
            }
        }

        if (episodesToAdd.Count > 0)
        {
            await dbContext.Episodes.AddRangeAsync(episodesToAdd, cancellationToken);
        }

        if (episodesToUpdate.Count > 0)
        {
            dbContext.Episodes.UpdateRange(episodesToUpdate);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return (episodesToAdd.Count, episodesToUpdate.Count);
    }

    private static Episode MapToDomain(ApiEpisode apiEpisode) => new()
    {
        Id = apiEpisode.Id,
        Name = apiEpisode.Name,
        EpisodeCode = apiEpisode.Episode,
        AirDate = apiEpisode.Air_date,
        Url = apiEpisode.Url,
        Characters = apiEpisode.Characters,
        CreatedAt = DateTime.UtcNow
    };
}