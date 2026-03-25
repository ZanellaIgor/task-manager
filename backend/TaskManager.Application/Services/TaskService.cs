using Mapster;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Filters;
using TaskManager.Application.Mappings;
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
        MapsterConfiguration.Configure();
        _taskRepository = taskRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<TaskDto>> GetAllAsync(TaskFilterDto filters)
    {
        var tasks = await _taskRepository.GetAllAsync(filters);
        return tasks.Adapt<IEnumerable<TaskDto>>();
    }

    public async Task<TaskDto> GetByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Tarefa {id} não encontrada.");

        return task.Adapt<TaskDto>();
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto dto)
    {
        if (!await _categoryRepository.ExistsActiveAsync(dto.CategoryId))
        {
            throw new BusinessException("Categoria não encontrada ou inativa.");
        }

        var task = dto.Adapt<TaskItem>();
        task.Status = TaskStatus.Pending;
        task.CompletedAt = null;

        var created = await _taskRepository.AddAsync(task);
        var persisted = await _taskRepository.GetByIdAsync(created.Id) ?? created;

        return persisted.Adapt<TaskDto>();
    }

    public async Task<TaskDto> UpdateAsync(int id, UpdateTaskDto dto)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Tarefa {id} não encontrada.");

        if (task.Status is TaskStatus.Completed)
        {
            throw new BusinessException("Tarefa concluída não pode ser editada.");
        }

        if (task.Status is TaskStatus.Cancelled)
        {
            throw new BusinessException("Tarefa cancelada não pode ser editada.");
        }

        if (!await _categoryRepository.ExistsActiveAsync(dto.CategoryId))
        {
            throw new BusinessException("Categoria não encontrada ou inativa.");
        }

        dto.Adapt(task);

        await _taskRepository.UpdateAsync(task);

        var updated = await _taskRepository.GetByIdAsync(id) ?? task;
        return updated.Adapt<TaskDto>();
    }

    public async Task<TaskDto> CompleteAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id)
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

        await _taskRepository.UpdateAsync(task);
        return task.Adapt<TaskDto>();
    }

    public async Task<TaskDto> CancelAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Tarefa {id} não encontrada.");

        if (task.Status is TaskStatus.Completed or TaskStatus.Cancelled)
        {
            throw new BusinessException("Tarefa não pode ser cancelada no status atual.");
        }

        task.Status = TaskStatus.Cancelled;
        task.CompletedAt = null;

        await _taskRepository.UpdateAsync(task);
        return task.Adapt<TaskDto>();
    }

    public async Task DeleteAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Tarefa {id} não encontrada.");

        if (task.Status is TaskStatus.Completed)
        {
            throw new BusinessException("Tarefa concluída não pode ser excluída.");
        }

        await _taskRepository.DeleteAsync(task);
    }
}
