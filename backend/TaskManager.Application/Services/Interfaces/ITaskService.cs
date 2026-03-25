using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Filters;

namespace TaskManager.Application.Services.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetAllAsync(TaskFilterDto filters);

    Task<TaskDto> GetByIdAsync(int id);

    Task<TaskDto> CreateAsync(CreateTaskDto dto);

    Task<TaskDto> UpdateAsync(int id, UpdateTaskDto dto);

    Task<TaskDto> CompleteAsync(int id);

    Task<TaskDto> CancelAsync(int id);

    Task DeleteAsync(int id);
}
