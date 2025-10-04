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
            .Produces<PaginatedResponse<CharacterDto>>(200)
            .ProducesProblem(500);
    }

    private static async Task<Results<Ok<PaginatedResponse<CharacterDto>>, ProblemHttpResult>> HandleAsync(
        IGetCharactersQuery query,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var queryResponse = await query.ExecuteAsync(page, pageSize, cancellationToken);
        
        var response = new PaginatedResponse<CharacterDto>(
            Data: queryResponse.Characters,
            Page: page,
            PageSize: pageSize,
            TotalCount: queryResponse.TotalCount,
            TotalPages: (int)Math.Ceiling(queryResponse.TotalCount / (double)pageSize),
            HasPreviousPage: page > 1,
            HasNextPage: page < (int)Math.Ceiling(queryResponse.TotalCount / (double)pageSize),
            FromDatabase: queryResponse.FromDatabase,
            LastFetchedAt: queryResponse.LastFetchedAt
        );

        return TypedResults.Ok(response);
    }
}