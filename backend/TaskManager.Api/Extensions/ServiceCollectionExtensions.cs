using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;
using TaskManager.Application.Repositories.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Application.Services.Interfaces;
using TaskManager.Application.Validators;
using TaskManager.Application.Mappings;
using TaskManager.Api.Options;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public const string FrontendCorsPolicyName = "FrontendCorsPolicy";

    public static IServiceCollection AddTaskManagerServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:Default must be configured.");
        }

        MapsterConfiguration.Configure();

        var frontendOptions = BindFrontendOptions(configuration);
        var frontendOrigins = frontendOptions.GetNormalizedOrigins();

        services.AddOptions<FrontendOptions>()
            .BindConfiguration(FrontendOptions.SectionName)
            .Validate(options => environment.IsDevelopment() || options.GetNormalizedOrigins().Length > 0,
                "Frontend:Origins must contain at least one valid origin outside Development.")
            .ValidateOnStart();

        services.AddOptions<RuntimeOptions>()
            .BindConfiguration(RuntimeOptions.SectionName)
            .ValidateOnStart();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString, sqlite => sqlite.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ICategoryService, CategoryService>();

        services.AddValidatorsFromAssemblyContaining<CreateTaskValidator>();
        services.AddFluentValidationAutoValidation();
        services.AddHealthChecks();
        services.AddHttpLogging(options =>
        {
            options.LoggingFields = HttpLoggingFields.RequestMethod
                | HttpLoggingFields.RequestPath
                | HttpLoggingFields.ResponseStatusCode
                | HttpLoggingFields.Duration;
        });

        services.AddCors(options =>
        {
            options.AddPolicy(FrontendCorsPolicyName, policy =>
            {
                if (frontendOrigins.Length == 0)
                {
                    return;
                }

                policy.WithOrigins(frontendOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(entry => entry.Value is not null && entry.Value.Errors.Count > 0)
                    .ToDictionary(
                        entry => string.IsNullOrWhiteSpace(entry.Key) ? "request" : entry.Key,
                        entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

                var problemDetails = ProblemDetailsExtensions.CreateValidationProblemDetails(context.HttpContext, errors);
                var result = new BadRequestObjectResult(problemDetails);
                result.ContentTypes.Add("application/problem+json");
                return result;
            };
        });

        return services;
    }

    private static FrontendOptions BindFrontendOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(FrontendOptions.SectionName);
        var options = section.Get<FrontendOptions>();

        if (options is not null && options.Origins.Length > 0)
        {
            return options;
        }

        var legacyOrigins = section["Origin"]?
            .Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? [];

        return new FrontendOptions { Origins = legacyOrigins };
    }
}
