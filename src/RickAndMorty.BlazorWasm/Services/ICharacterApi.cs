using Refit;
using RickAndMorty.BlazorWasm.Models;

namespace RickAndMorty.BlazorWasm.Services;

public interface ICharacterApi
{
    [Get("/api/characters")]
    Task<PaginatedResponse<List<CharacterDto>>> GetAllCharactersAsync(
        [Query] int page = 1,
        [Query] int pageSize = 50);

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