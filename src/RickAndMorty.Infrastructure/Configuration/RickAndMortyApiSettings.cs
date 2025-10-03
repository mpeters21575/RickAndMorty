namespace RickAndMorty.Infrastructure.Configuration;

public sealed class RickAndMortyApiSettings
{
    public const string SectionName = "RickAndMortyApi";

    public string BaseUrl { get; set; } = "https://rickandmortyapi.com/api";
    public string CharacterEndpoint { get; set; } = "/character";

    public string GetCharacterUrl(int? page = null)
    {
        var url = $"{BaseUrl.TrimEnd('/')}{CharacterEndpoint}";
        return page.HasValue ? $"{url}?page={page.Value}" : url;
    }
}