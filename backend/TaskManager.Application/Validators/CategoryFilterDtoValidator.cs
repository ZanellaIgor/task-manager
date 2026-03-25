using FluentValidation;
using TaskManager.Application.Filters;

namespace TaskManager.Application.Validators;

public sealed class CategoryFilterDtoValidator : AbstractValidator<CategoryFilterDto>
{
    private static readonly string[] AllowedSortFields = ["name", "createdAt", "updatedAt"];

    public CategoryFilterDtoValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than zero.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Search))
            .WithMessage("Search must have at most 100 characters.");

        RuleFor(x => x.SortBy)
            .Must(sortBy => string.IsNullOrWhiteSpace(sortBy) || AllowedSortFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortFields)}.");
    }
}
