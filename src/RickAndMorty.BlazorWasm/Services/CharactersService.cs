using RickAndMorty.BlazorWasm.Models;

namespace RickAndMorty.BlazorWasm.Services;

public interface ICharacterService
{
    Task<PaginatedResponse<List<CharacterDto>>> GetAllCharactersAsync(int page = 1, int pageSize = 50);
    Task<List<CharacterDto>> GetFilteredCharactersAsync(string? name = null, string? status = null, string? species = null);
    Task<List<CharacterDto>> GetCharactersByPlanetAsync(string planetName);
    Task<int> CreateCharacterAsync(CreateCharacterRequest request);
    Task<List<EpisodeDto>> GetAllEpisodesAsync();
}

public sealed class CharacterService(ICharacterApi api) : ICharacterService
{
    public Task<PaginatedResponse<List<CharacterDto>>> GetAllCharactersAsync(int page = 1, int pageSize = 50) =>
        api.GetAllCharactersAsync(page, pageSize);

    public Task<List<CharacterDto>> GetFilteredCharactersAsync(string? name = null, string? status = null, string? species = null) =>
        api.GetFilteredCharactersAsync(name, status, species);

    public Task<List<CharacterDto>> GetCharactersByPlanetAsync(string planetName) => 
        api.GetCharactersByPlanetAsync(planetName);

    public async Task<int> CreateCharacterAsync(CreateCharacterRequest request)
    {
        var response = await api.CreateCharacterAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        return int.Parse(content);
    }
    
    public Task<List<EpisodeDto>> GetAllEpisodesAsync() => 
        api.GetAllEpisodesAsync();
}