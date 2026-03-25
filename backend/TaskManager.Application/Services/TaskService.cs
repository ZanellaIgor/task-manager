using Mapster;
using TaskManager.Application.Common.Pagination;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Filters;
using TaskManager.Application.Repositories.Interfaces;
using TaskManager.Application.Services.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ICategoryRepository _categoryRepository;

    public TaskService(ITaskRepository taskRepository, ICategoryRepository categoryRepository)
    {
        _taskRepository = taskRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<PagedResult<TaskDto>> GetAllAsync(TaskFilterDto filters, CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetAllAsync(filters, cancellationToken);
        return PagedResult<TaskDto>.Create(tasks.Items.Adapt<List<TaskDto>>(), tasks.Page, tasks.PageSize, tasks.TotalItems);
    }

    public async Task<TaskOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var overview = await _taskRepository.GetOverviewAsync(5, 3, cancellationToken);

        return new TaskOverviewDto(
            overview.TotalCount,
            overview.PendingCount,
            overview.InProgressCount,
            overview.CompletedCount,
            overview.RecentTasks.Adapt<List<TaskDto>>(),
            overview.UpcomingTasks.Adapt<List<TaskDto>>());
    }

    public async Task<TaskDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Tarefa {id} não encontrada.");

        return task.Adapt<TaskDto>();
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _categoryRepository.ExistsActiveAsync(dto.CategoryId, cancellationToken))
        {
            throw new BusinessException("Categoria não encontrada ou inativa.");
        }

        var task = dto.Adapt<TaskItem>();
        task.Status = TaskStatus.Pending;
        task.CompletedAt = null;

        var created = await _taskRepository.AddAsync(task, cancellationToken);
        var persisted = await _taskRepository.GetByIdAsync(created.Id, cancellationToken) ?? created;

        return persisted.Adapt<TaskDto>();
    }

    public async Task<TaskDto> UpdateAsync(int id, UpdateTaskDto dto, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Tarefa {id} não encontrada.");

        if (task.Status is TaskStatus.Completed)
        {
            throw new BusinessException("Tarefa concluída não pode ser editada.");
        }

        if (task.Status is TaskStatus.Cancelled)
        {
            throw new BusinessException("Tarefa cancelada não pode ser editada.");
        }

        if (!await _categoryRepository.ExistsActiveAsync(dto.CategoryId, cancellationToken))
        {
            throw new BusinessException("Categoria não encontrada ou inativa.");
        }

        dto.Adapt(task);

        await _taskRepository.UpdateAsync(task, cancellationToken);

        var updated = await _taskRepository.GetByIdAsync(id, cancellationToken) ?? task;
        return updated.Adapt<TaskDto>();
    }

    public async Task<TaskDto> CompleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Tarefa {id} não encontrada.");

        if (task.Status is TaskStatus.Completed)
        {
            throw new BusinessException("Tarefa já está concluída.");
        }

        if (task.Status is TaskStatus.Cancelled)
        {
            throw new BusinessException("Tarefa cancelada não pode ser concluída.");
        }

        task.Status = TaskStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task, cancellationToken);
        return task.Adapt<TaskDto>();
    }

    public async Task<TaskDto> CancelAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Tarefa {id} não encontrada.");

        if (task.Status is TaskStatus.Completed or TaskStatus.Cancelled)
        {
            throw new BusinessException("Tarefa não pode ser cancelada no status atual.");
        }

        task.Status = TaskStatus.Cancelled;
        task.CompletedAt = null;

        await _taskRepository.UpdateAsync(task, cancellationToken);
        return task.Adapt<TaskDto>();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Tarefa {id} não encontrada.");

        if (task.Status is TaskStatus.Completed)
        {
            throw new BusinessException("Tarefa concluída não pode ser excluída.");
        }

        await _taskRepository.DeleteAsync(task, cancellationToken);
    }
}
