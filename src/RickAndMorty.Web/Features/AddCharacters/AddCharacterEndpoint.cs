using Carter;
using Microsoft.AspNetCore.Http.HttpResults;

namespace RickAndMorty.Web.Features.AddCharacters;

public sealed class AddCharacterEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/characters", HandleAsync)
            .WithName("AddCharacter")
            .WithTags("Characters")
            .WithOpenApi()
            .Produces<int>(201)
            .ProducesValidationProblem(400);
    }

    private static async Task<Results<Created<int>, BadRequest<string>>> HandleAsync(
        AddCharacterRequest request,
        IAddCharacterCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return TypedResults.BadRequest("Name is required");
        }

        var characterId = await command.ExecuteAsync(request, cancellationToken);
        
        return TypedResults.Created($"/api/characters/{characterId}", characterId);
    }
}