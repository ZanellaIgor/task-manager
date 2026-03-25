using TaskManager.Application.Common.Pagination;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Filters;
using TaskManager.Application.Repositories.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TaskItem>> GetAllAsync(TaskFilterDto filters, CancellationToken cancellationToken = default)
    {
        var query = _context.Tasks
            .AsNoTracking()
            .Include(task => task.Category)
            .AsQueryable();

        if (filters.Status is not null)
        {
            query = query.Where(task => task.Status == filters.Status);
        }

        if (filters.Priority is not null)
        {
            query = query.Where(task => task.Priority == filters.Priority);
        }

        if (filters.CategoryId is not null)
        {
            query = query.Where(task => task.CategoryId == filters.CategoryId);
        }

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search = $"%{filters.Search.Trim()}%";
            query = query.Where(task =>
                EF.Functions.Like(task.Title, search)
                || (task.Description != null && EF.Functions.Like(task.Description, search))
                || EF.Functions.Like(task.Category.Name, search));
        }

        query = ApplySorting(query, filters);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<TaskItem>.Create(items, filters.Page, filters.PageSize, totalItems);
    }

    public async Task<TaskOverviewResult> GetOverviewAsync(int recentLimit, int upcomingWindowDays, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var upcomingLimit = now.AddDays(upcomingWindowDays);

        var totalCount = await _context.Tasks.CountAsync(cancellationToken);
        var pendingCount = await _context.Tasks.CountAsync(task => task.Status == TaskStatus.Pending, cancellationToken);
        var inProgressCount = await _context.Tasks.CountAsync(task => task.Status == TaskStatus.InProgress, cancellationToken);
        var completedCount = await _context.Tasks.CountAsync(task => task.Status == TaskStatus.Completed, cancellationToken);

        var recentTasks = await _context.Tasks
            .AsNoTracking()
            .Include(task => task.Category)
            .OrderByDescending(task => task.UpdatedAt)
            .ThenByDescending(task => task.Id)
            .Take(recentLimit)
            .ToListAsync(cancellationToken);

        var upcomingTasks = await _context.Tasks
            .AsNoTracking()
            .Include(task => task.Category)
            .Where(task =>
                task.DueDate.HasValue
                && task.DueDate.Value >= now
                && task.DueDate.Value <= upcomingLimit
                && task.Status != TaskStatus.Completed
                && task.Status != TaskStatus.Cancelled)
            .OrderBy(task => task.DueDate)
            .ThenBy(task => task.Id)
            .Take(recentLimit)
            .ToListAsync(cancellationToken);

        return new TaskOverviewResult(
            totalCount,
            pendingCount,
            inProgressCount,
            completedCount,
            recentTasks,
            upcomingTasks);
    }

    public Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Tasks
            .Include(task => task.Category)
            .FirstOrDefaultAsync(task => task.Id == id, cancellationToken);
    }

    public async Task<TaskItem> AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<TaskItem> ApplySorting(IQueryable<TaskItem> query, TaskFilterDto filters)
    {
        var isAscending = filters.SortDirection == SortDirection.Asc;
        var sortBy = filters.SortBy?.Trim().ToLowerInvariant();

        return sortBy switch
        {
            "title" => isAscending
                ? query.OrderBy(task => task.Title).ThenBy(task => task.Id)
                : query.OrderByDescending(task => task.Title).ThenByDescending(task => task.Id),
            "updatedat" => isAscending
                ? query.OrderBy(task => task.UpdatedAt).ThenBy(task => task.Id)
                : query.OrderByDescending(task => task.UpdatedAt).ThenByDescending(task => task.Id),
            "duedate" => isAscending
                ? query.OrderBy(task => task.DueDate == null).ThenBy(task => task.DueDate).ThenBy(task => task.Id)
                : query.OrderBy(task => task.DueDate == null).ThenByDescending(task => task.DueDate).ThenByDescending(task => task.Id),
            "priority" => isAscending
                ? query.OrderBy(task => task.Priority).ThenBy(task => task.Id)
                : query.OrderByDescending(task => task.Priority).ThenByDescending(task => task.Id),
            "status" => isAscending
                ? query.OrderBy(task => task.Status).ThenBy(task => task.Id)
                : query.OrderByDescending(task => task.Status).ThenByDescending(task => task.Id),
            _ => isAscending
                ? query.OrderBy(task => task.CreatedAt).ThenBy(task => task.Id)
                : query.OrderByDescending(task => task.CreatedAt).ThenByDescending(task => task.Id),
        };
    }
}
