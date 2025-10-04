namespace RickAndMorty.Web.CrossCutting.Models;

public sealed record CharacterDto(
    int Id,
    string Name,
    string Status,
    string Species,
    string OriginName,
    string ImageUrl
);