using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using RickAndMorty.Web.CrossCutting.Models;

namespace RickAndMorty.Web.Features.GetCharactersByPlanet;

public sealed class GetCharactersByPlanetEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/characters/planet/{planetName}", HandleAsync)
            .WithName("GetCharactersByPlanet")
            .WithTags("Characters")
            .WithOpenApi()
            .Produces<List<CharacterDto>>(200)
            .ProducesProblem(500);
    }

    private static async Task<Ok<List<CharacterDto>>> HandleAsync(
        string planetName,
        IGetCharactersByPlanetQuery query,
        CancellationToken cancellationToken)
    {
        var characters = await query.ExecuteAsync(planetName, cancellationToken);
        return TypedResults.Ok(characters);
    }
}