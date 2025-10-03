using RickAndMorty.Web.CrossCutting.Models;

namespace RickAndMorty.Web.Features.GetCharacters;

public sealed record GetCharactersResponse(
    List<CharacterDto> Characters,
    bool FromDatabase,
    DateTime? LastFetchedAt
);