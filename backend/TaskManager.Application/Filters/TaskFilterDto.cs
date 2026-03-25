using TaskManager.Application.Common.Pagination;
using TaskPriority = TaskManager.Domain.Enums.TaskPriority;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.Filters;

public sealed record TaskFilterDto : PagedQuery
{
    public TaskStatus? Status { get; init; }

    public TaskPriority? Priority { get; init; }

    public int? CategoryId { get; init; }
}
