namespace TaskManager.Application.DTOs.Tasks;

public sealed record TaskOverviewDto(
    int TotalCount,
    int PendingCount,
    int InProgressCount,
    int CompletedCount,
    IReadOnlyList<TaskDto> RecentTasks,
    IReadOnlyList<TaskDto> UpcomingTasks);
