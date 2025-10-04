using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RickAndMorty.Web.CrossCutting.Models;

namespace RickAndMorty.Web.Features.GetFilteredCharacters;

public sealed class GetFilteredCharactersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/characters/filter", HandleAsync)
            .WithName("GetFilteredCharacters")
            .WithTags("Characters")
            .WithOpenApi()
            .Produces<List<CharacterDto>>(200)
            .ProducesProblem(500);
    }

    private static async Task<Ok<List<CharacterDto>>> HandleAsync(
        [FromQuery] string? name,
        [FromQuery] string? status,
        [FromQuery] string? species,
        IGetFilteredCharactersQuery query,
        CancellationToken cancellationToken)
    {
        var characters = await query.ExecuteAsync(name, status, species, cancellationToken);
        return TypedResults.Ok(characters);
    }
}