namespace RickAndMorty.BlazorWasm.Models;

public sealed record EpisodeDto(
    int Id,
    string Name,
    string Episode,
    string AirDate
);