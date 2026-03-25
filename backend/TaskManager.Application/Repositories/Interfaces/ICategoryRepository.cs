using TaskManager.Application.Common.Pagination;
using TaskManager.Application.Filters;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<PagedResult<Category>> GetAllAsync(CategoryFilterDto filters, CancellationToken cancellationToken = default);

    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);

    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);

    Task DeleteAsync(Category category, CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
}
