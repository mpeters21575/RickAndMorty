namespace RickAndMorty.BlazorWasm.Models;

public sealed record PaginatedResponse<T>(
    T Data,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage,
    bool FromDatabase,
    DateTime? LastFetchedAt
);