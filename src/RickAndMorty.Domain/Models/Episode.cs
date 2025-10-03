namespace RickAndMorty.Domain.Models;

public sealed class Episode
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string EpisodeCode { get; init; } = string.Empty;
    public string AirDate { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}