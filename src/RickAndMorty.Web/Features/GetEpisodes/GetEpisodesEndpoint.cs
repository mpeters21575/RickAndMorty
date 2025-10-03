using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using RickAndMorty.Web.CrossCutting.Models;

namespace RickAndMorty.Web.Features.GetEpisodes;

public sealed class GetEpisodesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/episodes", HandleAsync)
            .WithName("GetEpisodes")
            .WithTags("Episodes")
            .WithOpenApi()
            .Produces<List<EpisodeDto>>(200)
            .ProducesProblem(500);
    }

    private static async Task<Ok<List<EpisodeDto>>> HandleAsync(
        IGetEpisodesQuery query,
        CancellationToken cancellationToken)
    {
        var episodes = await query.ExecuteAsync(cancellationToken);
        return TypedResults.Ok(episodes);
    }
}