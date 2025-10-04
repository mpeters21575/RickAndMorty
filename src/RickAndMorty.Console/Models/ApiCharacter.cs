namespace RickAndMorty.Console.Models;

public sealed record ApiCharacter(
    int Id,
    string Name,
    string Status,
    string Species,
    string Image,
    ApiLocation Origin
);