using TaskManager.Domain.Entities;

namespace TaskManager.Application.DTOs.Tasks;

public sealed record TaskOverviewResult(
    int TotalCount,
    int PendingCount,
    int InProgressCount,
    int CompletedCount,
    IReadOnlyList<TaskItem> RecentTasks,
    IReadOnlyList<TaskItem> UpcomingTasks);
