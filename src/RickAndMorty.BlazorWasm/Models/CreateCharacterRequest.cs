namespace RickAndMorty.BlazorWasm.Models;

public sealed record CreateCharacterRequest(
    string Name,
    string Status,
    string Species,
    string OriginName
);