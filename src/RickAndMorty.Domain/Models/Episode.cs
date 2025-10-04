namespace RickAndMorty.Domain.Models;

public sealed class Episode
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string EpisodeCode { get; init; } = string.Empty;
    public string AirDate { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public List<string> Characters { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}