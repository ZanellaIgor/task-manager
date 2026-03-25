using TaskManager.Api.Extensions;
using TaskManager.Api.Middlewares;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddTaskManagerServices(builder.Configuration, builder.Environment);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await app.ApplyDatabaseSetupAsync();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors(ServiceCollectionExtensions.FrontendCorsPolicyName);
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
