namespace RickAndMorty.Web.Features.AddCharacters;

public sealed record AddCharacterRequest(
    string Name,
    string Status,
    string Species,
    string OriginName
);