using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RickAndMorty.Infrastructure;
using RickAndMorty.Infrastructure.Configuration;
using RickAndMorty.Web.CrossCutting;
using RickAndMorty.Web.CrossCutting.Models;

namespace RickAndMorty.Web.Features.GetCharacters;

public interface IGetCharactersQuery
{
    Task<GetCharactersResponse> ExecuteAsync(CancellationToken cancellationToken = default);
}

public sealed class GetCharactersQuery(
    AppDbContext dbContext, 
    IMemoryCache cache,
    IOptions<CharacterMonitorSettings> monitorSettings) : IGetCharactersQuery
{
    private readonly CharacterMonitorSettings _monitorSettings = monitorSettings.Value;

    public async Task<GetCharactersResponse> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKeys.Characters, out CachedCharacters? cached) && cached is not null)
        {
            return new GetCharactersResponse(cached.Characters, false, cached.FetchedAt);
        }

        var characters = await dbContext.Characters
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CharacterDto(
                c.Id,
                c.Name,
                c.Status,
                c.Species,
                c.Origin.Name,
                c.ImageUrl
            ))
            .ToListAsync(cancellationToken);

        var cachedData = new CachedCharacters(characters, DateTime.UtcNow);
        
        var cacheExpiration = TimeSpan.FromMinutes(_monitorSettings.IntervalMinutes);
        cache.Set(CacheKeys.Characters, cachedData, cacheExpiration);

        return new GetCharactersResponse(characters, true, cachedData.FetchedAt);
    }
}