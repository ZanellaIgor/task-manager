using FluentValidation;
using TaskManager.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace TaskManager.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, IHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (BusinessException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status422UnprocessableEntity, ex.Message);
        }
        catch (ValidationException ex)
        {
            var message = ex.Errors.Select(error => error.ErrorMessage).FirstOrDefault() ?? "Dados invalidos.";
            await WriteErrorAsync(context, StatusCodes.Status422UnprocessableEntity, message);
        }
        catch (Exception ex)
        {
            var payload = new Dictionary<string, object?>
            {
                ["error"] = "Erro interno."
            };

            if (_environment.IsDevelopment())
            {
                payload["detail"] = ex.Message;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(payload);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = message });
    }
}
