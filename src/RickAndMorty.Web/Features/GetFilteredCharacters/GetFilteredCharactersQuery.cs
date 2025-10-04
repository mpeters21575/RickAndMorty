using Microsoft.EntityFrameworkCore;
using RickAndMorty.Infrastructure;
using RickAndMorty.Web.CrossCutting.Models;

namespace RickAndMorty.Web.Features.GetFilteredCharacters;

public interface IGetFilteredCharactersQuery
{
    Task<List<CharacterDto>> ExecuteAsync(
        string? name,
        string? status,
        string? species,
        CancellationToken cancellationToken = default);
}

public sealed class GetFilteredCharactersQuery(AppDbContext dbContext) : IGetFilteredCharactersQuery
{
    public async Task<List<CharacterDto>> ExecuteAsync(
        string? name,
        string? status,
        string? species,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Characters.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{name}%"));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(c => c.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(species))
        {
            query = query.Where(c => EF.Functions.Like(c.Species, $"%{species}%"));
        }

        var characters = await query
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

        return characters;
    }
}