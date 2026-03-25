using Mapster;
using TaskManager.Application.DTOs.Categories;
using TaskManager.Application.Mappings;
using TaskManager.Application.Repositories.Interfaces;
using TaskManager.Application.Services.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        MapsterConfiguration.Configure();
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Adapt<IEnumerable<CategoryDto>>();
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Categoria {id} não encontrada.");

        return category.Adapt<CategoryDto>();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var normalizedName = dto.Name.Trim();

        if (await _categoryRepository.ExistsWithNameAsync(normalizedName))
        {
            throw new BusinessException("Já existe uma categoria com esse nome.");
        }

        var category = dto.Adapt<Category>();
        var created = await _categoryRepository.AddAsync(category);
        return created.Adapt<CategoryDto>();
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Categoria {id} não encontrada.");

        var normalizedName = dto.Name.Trim();

        if (await _categoryRepository.ExistsWithNameAsync(normalizedName, id))
        {
            throw new BusinessException("Já existe uma categoria com esse nome.");
        }

        dto.Adapt(category);

        await _categoryRepository.UpdateAsync(category);
        return category.Adapt<CategoryDto>();
    }

    public async Task<CategoryDto> DeactivateAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Categoria {id} não encontrada.");

        if (!category.IsActive)
        {
            return category.Adapt<CategoryDto>();
        }

        category.IsActive = false;

        await _categoryRepository.UpdateAsync(category);
        return category.Adapt<CategoryDto>();
    }

    public async Task<CategoryDto> ActivateAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Categoria {id} não encontrada.");

        if (category.IsActive)
        {
            return category.Adapt<CategoryDto>();
        }

        category.IsActive = true;

        await _categoryRepository.UpdateAsync(category);
        return category.Adapt<CategoryDto>();
    }
}
