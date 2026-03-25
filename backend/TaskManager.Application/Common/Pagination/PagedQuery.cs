namespace TaskManager.Application.Common.Pagination;

public abstract record PagedQuery
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }

    public string? SortBy { get; init; }

    public SortDirection SortDirection { get; init; } = SortDirection.Desc;
}
