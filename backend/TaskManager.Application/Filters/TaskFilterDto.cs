using TaskPriority = TaskManager.Domain.Enums.TaskPriority;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.Filters;

public record TaskFilterDto(
    TaskStatus? Status,
    TaskPriority? Priority,
    int? CategoryId,
    string? Search
);
