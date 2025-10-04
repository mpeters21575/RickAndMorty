namespace RickAndMorty.BlazorWasm.Models;

public sealed class CharacterNotification
{
    public Guid Id { get; init; }
    public string CharacterName { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public bool IsRead { get; set; }
}