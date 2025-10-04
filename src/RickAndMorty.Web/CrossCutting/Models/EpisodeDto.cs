namespace RickAndMorty.Web.CrossCutting.Models;

public sealed record EpisodeDto(
    int Id,
    string Name,
    string Episode,
    string AirDate,
    int CharacterCount
);