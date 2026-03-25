using TaskManager.Application.Common.Pagination;

namespace TaskManager.Application.Filters;

public sealed record CategoryFilterDto : PagedQuery
{
    public bool? IsActive { get; init; }
}
