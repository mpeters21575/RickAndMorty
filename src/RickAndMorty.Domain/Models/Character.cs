namespace RickAndMorty.Domain.Models;

public sealed class Character
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Species { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public Location Origin { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}