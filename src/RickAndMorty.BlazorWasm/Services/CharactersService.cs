using RickAndMorty.BlazorWasm.Models;

namespace RickAndMorty.BlazorWasm.Services;

public interface ICharacterService
{
    Task<List<CharacterDto>> GetAllCharactersAsync();
    Task<List<CharacterDto>> GetCharactersByPlanetAsync(string planetName);
    Task<int> CreateCharacterAsync(CreateCharacterRequest request);
    Task<List<EpisodeDto>> GetAllEpisodesAsync();
}

public sealed class CharacterService(ICharacterApi api) : ICharacterService
{
    public Task<List<CharacterDto>> GetAllCharactersAsync() => 
        api.GetAllCharactersAsync();

    public Task<List<CharacterDto>> GetCharactersByPlanetAsync(string planetName) => 
        api.GetCharactersByPlanetAsync(planetName);

    public Task<int> CreateCharacterAsync(CreateCharacterRequest request) => 
        api.CreateCharacterAsync(request);
    
    public Task<List<EpisodeDto>> GetAllEpisodesAsync() => 
        api.GetAllEpisodesAsync();
}