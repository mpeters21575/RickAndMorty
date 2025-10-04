using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RickAndMorty.Domain.Models;
using RickAndMorty.Infrastructure;
using RickAndMorty.Web.CrossCutting;

namespace RickAndMorty.Web.Features.AddCharacters;

public interface IAddCharacterCommand
{
    Task<int> ExecuteAsync(AddCharacterRequest request, CancellationToken cancellationToken = default);
}

public sealed class AddCharacterCommand(
    AppDbContext dbContext, 
    IMemoryCache cache) : IAddCharacterCommand
{
    public async Task<int> ExecuteAsync(AddCharacterRequest request, CancellationToken cancellationToken = default)
    {
        var maxId = await dbContext.Characters
            .OrderByDescending(c => c.Id)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var character = new Character
        {
            Id = maxId + 1,
            Name = request.Name,
            Status = request.Status,
            Species = request.Species,
            ImageUrl = string.Empty,
            Origin = new Location { Name = request.OriginName, Url = string.Empty },
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.Characters.AddAsync(character, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        cache.RemoveByPrefix(CacheKeys.Characters);

        return character.Id;
    }
}