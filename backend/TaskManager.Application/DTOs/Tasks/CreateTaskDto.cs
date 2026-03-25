using TaskPriority = TaskManager.Domain.Enums.TaskPriority;

namespace TaskManager.Application.DTOs.Tasks;

public record CreateTaskDto(
    string Title,
    string? Description,
    int CategoryId,
    TaskPriority Priority,
    DateTime? DueDate
);
