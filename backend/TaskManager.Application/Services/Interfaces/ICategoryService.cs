using TaskManager.Application.DTOs.Categories;

namespace TaskManager.Application.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync();

    Task<CategoryDto> GetByIdAsync(int id);

    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);

    Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto);

    Task<CategoryDto> DeactivateAsync(int id);

    Task<CategoryDto> ActivateAsync(int id);
}
