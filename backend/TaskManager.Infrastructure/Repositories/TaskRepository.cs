using TaskManager.Application.Filters;
using TaskManager.Application.Repositories.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync(TaskFilterDto filters, CancellationToken cancellationToken = default)
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
            var search = filters.Search.Trim().ToLower();
            query = query.Where(task =>
                task.Title.ToLower().Contains(search)
                || (task.Description != null && task.Description.ToLower().Contains(search))
                || task.Category.Name.ToLower().Contains(search));
        }

        return await query
            .OrderByDescending(task => task.CreatedAt)
            .ToListAsync(cancellationToken);
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
}
