using Carter;
using FluentValidation;
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

    private static async Task<Results<Created<int>, ValidationProblem>> HandleAsync(
        AddCharacterRequest request,
        IValidator<AddCharacterRequest> validator,
        IAddCharacterCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            
            return TypedResults.ValidationProblem(errors);
        }

        var characterId = await command.ExecuteAsync(request, cancellationToken);
        
        return TypedResults.Created($"/api/characters/{characterId}", characterId);
    }
}