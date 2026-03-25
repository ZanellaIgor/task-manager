using TaskManager.Application.Common.Pagination;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Filters;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<PagedResult<TaskItem>> GetAllAsync(TaskFilterDto filters, CancellationToken cancellationToken = default);

    Task<TaskOverviewResult> GetOverviewAsync(int recentLimit, int upcomingWindowDays, CancellationToken cancellationToken = default);

    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TaskItem> AddAsync(TaskItem task, CancellationToken cancellationToken = default);

    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);

    Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default);
}
