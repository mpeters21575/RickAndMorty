using Refit;
using RickAndMorty.BlazorWasm.Models;

namespace RickAndMorty.BlazorWasm.Services;

public interface ICharacterApi
{
    [Get("/api/characters")]
    Task<List<CharacterDto>> GetAllCharactersAsync();

    [Get("/api/characters/planet/{planetName}")]
    Task<List<CharacterDto>> GetCharactersByPlanetAsync(string planetName);

    [Post("/api/characters")]
    Task<int> CreateCharacterAsync([Body] CreateCharacterRequest request);
    
    [Get("/api/episodes")]
    Task<List<EpisodeDto>> GetAllEpisodesAsync();
}