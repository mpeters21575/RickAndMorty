namespace RickAndMorty.Console.Models;

public sealed record ApiEpisodesResponse(
    List<ApiEpisode> Results,
    ApiInfo Info
);