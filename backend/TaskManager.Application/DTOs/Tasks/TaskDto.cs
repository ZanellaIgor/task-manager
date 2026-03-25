namespace TaskManager.Application.DTOs.Tasks;

public record CategorySummaryDto(int Id, string Name);

public record TaskDto(
    int Id,
    string Title,
    string? Description,
    int CategoryId,
    CategorySummaryDto? Category,
    string Status,
    string Priority,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? DueDate,
    DateTime? CompletedAt
);
