namespace TaskManager.Api.Options;

public sealed class RuntimeOptions
{
    public const string SectionName = "Runtime";

    public bool ApplyMigrationsOnStartup { get; init; } = true;

    public bool SeedOnStartup { get; init; } = true;
}
