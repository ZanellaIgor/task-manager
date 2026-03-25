using Mapster;
using TaskManager.Application.Common.Pagination;
using TaskManager.Application.DTOs.Categories;
using TaskManager.Application.Filters;
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
        _categoryRepository = categoryRepository;
    }

    public async Task<PagedResult<CategoryDto>> GetAllAsync(CategoryFilterDto filters, CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllAsync(filters, cancellationToken);
        return PagedResult<CategoryDto>.Create(categories.Items.Adapt<List<CategoryDto>>(), categories.Page, categories.PageSize, categories.TotalItems);
    }

    public async Task<CategoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Categoria {id} não encontrada.");

        return category.Adapt<CategoryDto>();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedName = dto.Name.Trim();

        if (await _categoryRepository.ExistsWithNameAsync(normalizedName, cancellationToken: cancellationToken))
        {
            throw new BusinessException("Já existe uma categoria com esse nome.");
        }

        var category = dto.Adapt<Category>();
        var created = await _categoryRepository.AddAsync(category, cancellationToken);
        return created.Adapt<CategoryDto>();
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Categoria {id} não encontrada.");

        var normalizedName = dto.Name.Trim();

        if (await _categoryRepository.ExistsWithNameAsync(normalizedName, id, cancellationToken))
        {
            throw new BusinessException("Já existe uma categoria com esse nome.");
        }

        dto.Adapt(category);

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        return category.Adapt<CategoryDto>();
    }

    public async Task<CategoryDto> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Categoria {id} não encontrada.");

        if (!category.IsActive)
        {
            return category.Adapt<CategoryDto>();
        }

        category.IsActive = false;

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        return category.Adapt<CategoryDto>();
    }

    public async Task<CategoryDto> ActivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Categoria {id} não encontrada.");

        if (category.IsActive)
        {
            return category.Adapt<CategoryDto>();
        }

        category.IsActive = true;

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        return category.Adapt<CategoryDto>();
    }
}
