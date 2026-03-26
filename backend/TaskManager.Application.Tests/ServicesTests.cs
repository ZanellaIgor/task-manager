using TaskManager.Application.Common.Pagination;
using TaskManager.Application.DTOs.Categories;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Filters;
using TaskManager.Application.Mappings;
using TaskManager.Application.Repositories.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;
using TaskPriority = TaskManager.Domain.Enums.TaskPriority;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;
using Xunit;

namespace TaskManager.Application.Tests;

public class ServicesTests
{
    public ServicesTests()
    {
        MapsterConfiguration.Configure();
    }

    [Fact]
    public async Task CreateTask_ShouldThrow_WhenCategoryIsInactive()
    {
        var taskRepository = new InMemoryTaskRepository();
        var categoryRepository = new InMemoryCategoryRepository();
        categoryRepository.Items.Add(new Category { Id = 10, Name = "Infra", IsActive = false });

        var service = new TaskService(taskRepository, categoryRepository);

        var action = () => service.CreateAsync(
            new CreateTaskDto("Ajustar deploy", "Atualizar variáveis de ambiente.", 10, TaskPriority.High, DateTime.UtcNow.AddDays(1)),
            CancellationToken.None);

        await Assert.ThrowsAsync<BusinessException>(action);
    }

    [Fact]
    public async Task CompleteTask_ShouldSetCompletedStatusAndTimestamp()
    {
        var taskRepository = new InMemoryTaskRepository();
        var categoryRepository = new InMemoryCategoryRepository();

        categoryRepository.Items.Add(new Category { Id = 1, Name = "Desenvolvimento", IsActive = true });
        taskRepository.Items.Add(new TaskItem
        {
            Id = 7,
            Title = "Refatorar filtros",
            CategoryId = 1,
            Category = categoryRepository.Items.Single(),
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.Medium,
        });

        var service = new TaskService(taskRepository, categoryRepository);

        var result = await service.CompleteAsync(7, CancellationToken.None);

        Assert.Equal(TaskStatus.Completed, result.Status);
        Assert.NotNull(result.CompletedAt);
    }

    [Fact]
    public async Task DeleteTask_ShouldThrow_WhenTaskIsCompleted()
    {
        var taskRepository = new InMemoryTaskRepository();
        var categoryRepository = new InMemoryCategoryRepository();

        taskRepository.Items.Add(new TaskItem
        {
            Id = 3,
            Title = "Publicar release notes",
            CategoryId = 1,
            Category = new Category { Id = 1, Name = "Documentação", IsActive = true },
            Status = TaskStatus.Completed,
            Priority = TaskPriority.Low,
            CompletedAt = DateTime.UtcNow.AddMinutes(-5),
        });

        var service = new TaskService(taskRepository, categoryRepository);

        var action = () => service.DeleteAsync(3, CancellationToken.None);

        await Assert.ThrowsAsync<BusinessException>(action);
    }

    [Fact]
    public async Task CreateCategory_ShouldThrow_WhenNameAlreadyExistsIgnoringCase()
    {
        var categoryRepository = new InMemoryCategoryRepository();
        categoryRepository.Items.Add(new Category { Id = 1, Name = "Documentação", IsActive = true });

        var service = new CategoryService(categoryRepository);

        var action = () => service.CreateAsync(
            new CreateCategoryDto(" documentação ", "Duplicada apenas por variação de caixa."),
            CancellationToken.None);

        await Assert.ThrowsAsync<BusinessException>(action);
    }

    [Fact]
    public async Task DeactivateCategory_ShouldReturnInactiveCategory()
    {
        var categoryRepository = new InMemoryCategoryRepository();
        categoryRepository.Items.Add(new Category { Id = 12, Name = "Produto", IsActive = true });

        var service = new CategoryService(categoryRepository);

        var result = await service.DeactivateAsync(12, CancellationToken.None);

        Assert.False(result.IsActive);
    }

    private sealed class InMemoryTaskRepository : ITaskRepository
    {
        public List<TaskItem> Items { get; } = [];

        public Task<PagedResult<TaskItem>> GetAllAsync(TaskFilterDto filters, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(PagedResult<TaskItem>.Create([], filters.Page, filters.PageSize, 0));
        }

        public Task<TaskOverviewResult> GetOverviewAsync(int recentLimit, int upcomingWindowDays, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new TaskOverviewResult(Items.Count, 0, 0, 0, [], []));
        }

        public Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Items.SingleOrDefault(item => item.Id == id));
        }

        public Task<TaskItem> AddAsync(TaskItem task, CancellationToken cancellationToken = default)
        {
            task.Id = task.Id == 0 ? Items.Count + 1 : task.Id;
            task.Category ??= new Category { Id = task.CategoryId, Name = "Categoria" };
            Items.Add(task);
            return Task.FromResult(task);
        }

        public Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
        {
            var index = Items.FindIndex(item => item.Id == task.Id);
            if (index >= 0)
            {
                Items[index] = task;
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default)
        {
            Items.Remove(task);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryCategoryRepository : ICategoryRepository
    {
        public List<Category> Items { get; } = [];

        public Task<PagedResult<Category>> GetAllAsync(CategoryFilterDto filters, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(PagedResult<Category>.Create([], filters.Page, filters.PageSize, 0));
        }

        public Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Items.SingleOrDefault(item => item.Id == id));
        }

        public Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default)
        {
            category.Id = category.Id == 0 ? Items.Count + 1 : category.Id;
            Items.Add(category);
            return Task.FromResult(category);
        }

        public Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
        {
            var index = Items.FindIndex(item => item.Id == category.Id);
            if (index >= 0)
            {
                Items[index] = category;
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Category category, CancellationToken cancellationToken = default)
        {
            Items.Remove(category);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsActiveAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Items.Any(category => category.Id == id && category.IsActive));
        }

        public Task<bool> ExistsWithNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var normalizedName = name.Trim().ToLowerInvariant();
            return Task.FromResult(Items.Any(category =>
                category.Name.Trim().ToLowerInvariant() == normalizedName
                && (!excludeId.HasValue || category.Id != excludeId.Value)));
        }
    }
}
