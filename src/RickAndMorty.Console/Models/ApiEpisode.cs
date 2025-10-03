namespace RickAndMorty.Console.Models;

public sealed record ApiEpisode(
    int Id,
    string Name,
    string Air_date,
    string Episode
);