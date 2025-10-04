namespace RickAndMorty.Web.CrossCutting.Models;

public sealed record PaginatedResponse<T>(
    List<T> Data,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage,
    bool FromDatabase,
    DateTime? LastFetchedAt
);