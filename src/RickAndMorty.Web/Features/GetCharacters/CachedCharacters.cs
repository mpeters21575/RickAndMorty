using RickAndMorty.Web.CrossCutting.Models;

namespace RickAndMorty.Web.Features.GetCharacters;

internal sealed record CachedCharacters(List<CharacterDto> Characters, DateTime FetchedAt);