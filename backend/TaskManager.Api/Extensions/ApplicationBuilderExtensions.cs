using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskManager.Api.Options;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task ApplyDatabaseSetupAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var runtimeOptions = scope.ServiceProvider.GetRequiredService<IOptions<RuntimeOptions>>().Value;

        if (!runtimeOptions.ApplyMigrationsOnStartup && !runtimeOptions.SeedOnStartup)
        {
            return;
        }

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (runtimeOptions.ApplyMigrationsOnStartup)
        {
            await db.Database.MigrateAsync();
        }

        if (runtimeOptions.SeedOnStartup)
        {
            await DatabaseSeeder.SeedAsync(db);
        }
    }
}
