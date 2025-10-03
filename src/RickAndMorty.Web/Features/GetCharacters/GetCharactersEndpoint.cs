using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using RickAndMorty.Web.CrossCutting.Models;

namespace RickAndMorty.Web.Features.GetCharacters;

public sealed class GetCharactersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/characters", HandleAsync)
            .WithName("GetCharacters")
            .WithTags("Characters")
            .WithOpenApi()
            .Produces<List<CharacterDto>>(200)
            .ProducesProblem(500);
    }

    private static async Task<Results<Ok<List<CharacterDto>>, ProblemHttpResult>> HandleAsync(
        IGetCharactersQuery query,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var response = await query.ExecuteAsync(cancellationToken);
        
        context.Response.Headers["from-database"] = response.FromDatabase.ToString().ToLowerInvariant();
        
        if (response.LastFetchedAt.HasValue)
        {
            context.Response.Headers["last-fetched-at"] = response.LastFetchedAt.Value.ToString("O");
        }

        return TypedResults.Ok(response.Characters);
    }
}