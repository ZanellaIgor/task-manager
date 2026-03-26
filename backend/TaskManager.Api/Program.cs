using TaskManager.Api.Extensions;
using TaskManager.Api.Middlewares;
using Microsoft.AspNetCore.HttpLogging;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddTaskManagerServices(builder.Configuration, builder.Environment);
builder.Services.AddTaskManagerOpenApi();

var app = builder.Build();

await app.ApplyDatabaseSetupAsync();

app.UseHttpLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors(ServiceCollectionExtensions.FrontendCorsPolicyName);
app.UseSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
