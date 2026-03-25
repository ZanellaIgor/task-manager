using Mapster;
using TaskManager.Application.DTOs.Categories;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Domain.Entities;
using TaskPriority = TaskManager.Domain.Enums.TaskPriority;

namespace TaskManager.Application.Mappings;

internal static class MapsterConfiguration
{
    private static int _configured;

    public static void Configure()
    {
        if (Interlocked.Exchange(ref _configured, 1) == 1)
        {
            return;
        }

        TypeAdapterConfig.GlobalSettings.NewConfig<CreateTaskDto, TaskItem>()
            .Map(dest => dest.Title, src => src.Title.Trim())
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest.Priority, src => Enum.Parse<TaskPriority>(src.Priority.Trim(), true))
            .Map(dest => dest.DueDate, src => src.DueDate);

        TypeAdapterConfig.GlobalSettings.NewConfig<UpdateTaskDto, TaskItem>()
            .Map(dest => dest.Title, src => src.Title.Trim())
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest.Priority, src => Enum.Parse<TaskPriority>(src.Priority.Trim(), true))
            .Map(dest => dest.DueDate, src => src.DueDate);

        TypeAdapterConfig.GlobalSettings.NewConfig<CreateCategoryDto, Category>()
            .Map(dest => dest.Name, src => src.Name.Trim())
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.IsActive, _ => true);

        TypeAdapterConfig.GlobalSettings.NewConfig<UpdateCategoryDto, Category>()
            .Map(dest => dest.Name, src => src.Name.Trim())
            .Map(dest => dest.Description, src => src.Description);

        TypeAdapterConfig.GlobalSettings.NewConfig<Category, CategorySummaryDto>();

        TypeAdapterConfig.GlobalSettings.NewConfig<Category, CategoryDto>();

        TypeAdapterConfig.GlobalSettings.NewConfig<TaskItem, TaskDto>()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Priority, src => src.Priority.ToString())
            .Map(dest => dest.Category, src => src.Category);
    }
}
