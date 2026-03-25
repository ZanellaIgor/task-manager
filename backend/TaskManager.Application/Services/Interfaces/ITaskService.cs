using TaskManager.Application.Common.Pagination;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Filters;

namespace TaskManager.Application.Services.Interfaces;

public interface ITaskService
{
    Task<PagedResult<TaskDto>> GetAllAsync(TaskFilterDto filters, CancellationToken cancellationToken = default);

    Task<TaskOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);

    Task<TaskDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TaskDto> CreateAsync(CreateTaskDto dto, CancellationToken cancellationToken = default);

    Task<TaskDto> UpdateAsync(int id, UpdateTaskDto dto, CancellationToken cancellationToken = default);

    Task<TaskDto> CompleteAsync(int id, CancellationToken cancellationToken = default);

    Task<TaskDto> CancelAsync(int id, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
