namespace TaskManager.Application.DTOs.Tasks;

public record CreateTaskDto(
    string Title,
    string? Description,
    int CategoryId,
    string Priority,
    DateTime? DueDate
);
