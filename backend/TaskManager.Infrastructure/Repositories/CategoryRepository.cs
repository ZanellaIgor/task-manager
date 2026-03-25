using TaskManager.Application.Common.Pagination;
using TaskManager.Application.Filters;
using TaskManager.Application.Repositories.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Category>> GetAllAsync(CategoryFilterDto filters, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories
            .AsNoTracking()
            .AsQueryable();

        if (filters.IsActive.HasValue)
        {
            query = query.Where(category => category.IsActive == filters.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search = $"%{filters.Search.Trim()}%";
            query = query.Where(category =>
                EF.Functions.Like(category.Name, search)
                || (category.Description != null && EF.Functions.Like(category.Description, search)));
        }

        query = ApplySorting(query, filters);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<Category>.Create(items, filters.Page, filters.PageSize, totalItems);
    }

    public Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Categories
            .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
    }

    public async Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Category category, CancellationToken cancellationToken = default)
    {
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsActiveAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Categories.AnyAsync(category => category.Id == id && category.IsActive, cancellationToken);
    }

    public Task<bool> ExistsWithNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLower();

        return _context.Categories.AnyAsync(category =>
            category.Name.ToLower() == normalizedName
            && (!excludeId.HasValue || category.Id != excludeId.Value), cancellationToken);
    }

    private static IQueryable<Category> ApplySorting(IQueryable<Category> query, CategoryFilterDto filters)
    {
        var isAscending = filters.SortDirection == SortDirection.Asc;
        var sortBy = filters.SortBy?.Trim().ToLowerInvariant();

        return sortBy switch
        {
            "createdat" => isAscending
                ? query.OrderBy(category => category.CreatedAt).ThenBy(category => category.Id)
                : query.OrderByDescending(category => category.CreatedAt).ThenByDescending(category => category.Id),
            "updatedat" => isAscending
                ? query.OrderBy(category => category.UpdatedAt).ThenBy(category => category.Id)
                : query.OrderByDescending(category => category.UpdatedAt).ThenByDescending(category => category.Id),
            _ => isAscending
                ? query.OrderBy(category => category.Name).ThenBy(category => category.Id)
                : query.OrderByDescending(category => category.Name).ThenByDescending(category => category.Id),
        };
    }
}
