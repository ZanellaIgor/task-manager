using TaskPriority = TaskManager.Domain.Enums.TaskPriority;

namespace TaskManager.Application.DTOs.Tasks;

public record UpdateTaskDto(
    string Title,
    string? Description,
    int CategoryId,
    TaskPriority Priority,
    DateTime? DueDate
);
