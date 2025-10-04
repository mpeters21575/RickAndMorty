using RickAndMorty.Web.CrossCutting.Models;
using Microsoft.EntityFrameworkCore;
using RickAndMorty.Infrastructure;

namespace RickAndMorty.Web.Features.GetCharactersByPlanet;

public interface IGetCharactersByPlanetQuery
{
    Task<List<CharacterDto>> ExecuteAsync(string planetName, CancellationToken cancellationToken = default);
}

public sealed class GetCharactersByPlanetQuery(AppDbContext dbContext) : IGetCharactersByPlanetQuery
{
    public async Task<List<CharacterDto>> ExecuteAsync(
        string planetName, 
        CancellationToken cancellationToken = default)
    {
        var searchPattern = $"%{planetName}%";
        
        return await dbContext.Characters
            .AsNoTracking()
            .Where(c => EF.Functions.Like(c.Origin.Name, searchPattern))
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
    }
}