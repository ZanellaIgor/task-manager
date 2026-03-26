using FluentValidation;
using TaskManager.Api.Extensions;
using TaskManager.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace TaskManager.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        IHostEnvironment environment,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _environment = environment;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Recurso não encontrado para {Method} {Path}. TraceId: {TraceId}",
                context.Request.Method, context.Request.Path, context.TraceIdentifier);

            var problemDetails = ProblemDetailsExtensions.CreateProblemDetails(
                context,
                StatusCodes.Status404NotFound,
                "Recurso não encontrado.",
                ex.Message,
                "resource_not_found");
            await WriteProblemDetailsAsync(context, problemDetails);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Regra de negócio violada em {Method} {Path}. TraceId: {TraceId}",
                context.Request.Method, context.Request.Path, context.TraceIdentifier);

            var problemDetails = ProblemDetailsExtensions.CreateProblemDetails(
                context,
                StatusCodes.Status422UnprocessableEntity,
                "Violação de regra de negócio.",
                ex.Message,
                "business_rule_violation");
            await WriteProblemDetailsAsync(context, problemDetails);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Falha de validação em {Method} {Path}. TraceId: {TraceId}",
                context.Request.Method, context.Request.Path, context.TraceIdentifier);

            var errors = ex.Errors
                .GroupBy(error => string.IsNullOrWhiteSpace(error.PropertyName) ? "request" : error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());

            var problemDetails = ProblemDetailsExtensions.CreateValidationProblemDetails(context, errors);
            await WriteProblemDetailsAsync(context, problemDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado em {Method} {Path}. TraceId: {TraceId}",
                context.Request.Method, context.Request.Path, context.TraceIdentifier);

            var problemDetails = ProblemDetailsExtensions.CreateProblemDetails(
                context,
                StatusCodes.Status500InternalServerError,
                "Erro inesperado.",
                "Ocorreu um erro inesperado.",
                "unexpected_error");

            if (_environment.IsDevelopment())
            {
                problemDetails.Extensions["exception"] = ex.Message;
            }

            await WriteProblemDetailsAsync(context, problemDetails);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, ProblemDetails problemDetails)
    {
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
