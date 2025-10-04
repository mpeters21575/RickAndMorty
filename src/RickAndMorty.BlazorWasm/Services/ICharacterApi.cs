using Refit;
using RickAndMorty.BlazorWasm.Models;

namespace RickAndMorty.BlazorWasm.Services;

public interface ICharacterApi
{
    [Get("/api/characters")]
    Task<List<CharacterDto>> GetAllCharactersAsync();

    [Get("/api/characters/filter")]
    Task<List<CharacterDto>> GetFilteredCharactersAsync(
        [Query] string? name = null,
        [Query] string? status = null,
        [Query] string? species = null);

    [Get("/api/characters/planet/{planetName}")]
    Task<List<CharacterDto>> GetCharactersByPlanetAsync(string planetName);

    [Post("/api/characters")]
    Task<HttpResponseMessage> CreateCharacterAsync([Body] CreateCharacterRequest request);
    
    [Get("/api/episodes")]
    Task<List<EpisodeDto>> GetAllEpisodesAsync();
}