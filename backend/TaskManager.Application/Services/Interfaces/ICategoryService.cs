using TaskManager.Application.Common.Pagination;
using TaskManager.Application.DTOs.Categories;
using TaskManager.Application.Filters;

namespace TaskManager.Application.Services.Interfaces;

public interface ICategoryService
{
    Task<PagedResult<CategoryDto>> GetAllAsync(CategoryFilterDto filters, CancellationToken cancellationToken = default);

    Task<CategoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);

    Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto, CancellationToken cancellationToken = default);

    Task<CategoryDto> DeactivateAsync(int id, CancellationToken cancellationToken = default);

    Task<CategoryDto> ActivateAsync(int id, CancellationToken cancellationToken = default);
}
