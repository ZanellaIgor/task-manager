namespace TaskManager.Application.DTOs.Tasks;

public record UpdateTaskDto(
    string Title,
    string? Description,
    int CategoryId,
    string Priority,
    DateTime? DueDate
);
