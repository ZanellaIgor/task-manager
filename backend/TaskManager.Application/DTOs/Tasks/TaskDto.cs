using TaskPriority = TaskManager.Domain.Enums.TaskPriority;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.DTOs.Tasks;

public record CategorySummaryDto(int Id, string Name);

public record TaskDto(
    int Id,
    string Title,
    string? Description,
    int CategoryId,
    CategorySummaryDto? Category,
    TaskStatus Status,
    TaskPriority Priority,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? DueDate,
    DateTime? CompletedAt
);
