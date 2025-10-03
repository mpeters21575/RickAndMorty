namespace RickAndMorty.Console.Models;

public sealed record ApiResponse(
    List<ApiCharacter> Results,
    ApiInfo Info
);