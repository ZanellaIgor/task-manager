namespace TaskManager.Application.DTOs.Categories;

public record CategoryDto(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
