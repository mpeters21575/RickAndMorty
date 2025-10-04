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
    Task<GetCharactersResponse> ExecuteAsync(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
}

public sealed class GetCharactersQuery(
    AppDbContext dbContext, 
    IMemoryCache cache,
    IOptions<CharacterMonitorSettings> monitorSettings) : IGetCharactersQuery
{
    public async Task<GetCharactersResponse> ExecuteAsync(
        int page = 1, 
        int pageSize = 50, 
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var cacheKey = $"{CacheKeys.Characters}_page_{page}_size_{pageSize}";

        if (cache.TryGetValue(cacheKey, out CachedCharacters? cached) && cached is not null)
        {
            return new GetCharactersResponse(cached.Characters, false, cached.FetchedAt, cached.TotalCount);
        }

        var totalCount = await dbContext.Characters.CountAsync(cancellationToken);

        var characters = await dbContext.Characters
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CharacterDto(
                c.Id,
                c.Name,
                c.Status,
                c.Species,
                c.Origin.Name,
                c.ImageUrl
            ))
            .ToListAsync(cancellationToken);

        var cachedData = new CachedCharacters(characters, DateTime.UtcNow, totalCount);
        
        cache.Set(cacheKey, cachedData, TimeSpan.FromMinutes(5));

        return new GetCharactersResponse(characters, true, cachedData.FetchedAt, totalCount);
    }
}