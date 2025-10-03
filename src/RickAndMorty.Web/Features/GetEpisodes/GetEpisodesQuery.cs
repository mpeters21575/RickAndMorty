using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RickAndMorty.Infrastructure;
using RickAndMorty.Web.CrossCutting;
using RickAndMorty.Web.CrossCutting.Models;

namespace RickAndMorty.Web.Features.GetEpisodes;

public interface IGetEpisodesQuery
{
    Task<List<EpisodeDto>> ExecuteAsync(CancellationToken cancellationToken = default);
}

public sealed class GetEpisodesQuery(AppDbContext dbContext, IMemoryCache cache) : IGetEpisodesQuery
{
    public async Task<List<EpisodeDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKeys.Episodes, out List<EpisodeDto>? cached) && cached is not null)
        {
            return cached;
        }

        var episodes = await dbContext.Episodes
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .Select(e => new EpisodeDto(
                e.Id,
                e.Name,
                e.EpisodeCode,
                e.AirDate
            ))
            .ToListAsync(cancellationToken);

        cache.Set(CacheKeys.Episodes, episodes, TimeSpan.FromMinutes(5));

        return episodes;
    }
}