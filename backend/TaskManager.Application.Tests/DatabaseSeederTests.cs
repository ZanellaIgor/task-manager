using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Data;
using Xunit;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.Tests;

public class DatabaseSeederTests
{
    [Fact]
    public async Task SeedAsync_ShouldCreateConsistentDataSet()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        await DatabaseSeeder.SeedAsync(context);

        var categories = await context.Categories.AsNoTracking().ToListAsync();
        var tasks = await context.Tasks.AsNoTracking().ToListAsync();
        var activeCategoryIds = categories
            .Where(category => category.IsActive)
            .Select(category => category.Id)
            .ToHashSet();

        Assert.True(tasks.Count >= 27, "Seed should provide at least 3 pages of tasks with page size 9.");
        Assert.True(categories.Count(category => category.IsActive) > 10, "Seed should provide more than one page of active categories with page size 10.");
        Assert.Equal(categories.Count, categories.Select(category => category.Name.Trim().ToLowerInvariant()).Distinct().Count());
        Assert.All(tasks, task => Assert.Contains(task.CategoryId, activeCategoryIds));
        Assert.All(tasks.Where(task => task.Status == TaskStatus.Completed), task => Assert.NotNull(task.CompletedAt));
        Assert.All(tasks.Where(task => task.Status != TaskStatus.Completed), task => Assert.Null(task.CompletedAt));
        Assert.DoesNotContain(tasks, task =>
            task.Status != TaskStatus.Completed &&
            task.DueDate.HasValue &&
            task.DueDate.Value <= DateTime.UtcNow);
    }

    [Fact]
    public async Task SeedAsync_ShouldBeIdempotent()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        await DatabaseSeeder.SeedAsync(context);
        var categoryCountAfterFirstRun = await context.Categories.CountAsync();
        var taskCountAfterFirstRun = await context.Tasks.CountAsync();

        await DatabaseSeeder.SeedAsync(context);

        Assert.Equal(categoryCountAfterFirstRun, await context.Categories.CountAsync());
        Assert.Equal(taskCountAfterFirstRun, await context.Tasks.CountAsync());
    }
}
