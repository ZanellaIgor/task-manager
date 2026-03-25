using FluentValidation;
using TaskManager.Api.Extensions;
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
            var problemDetails = ProblemDetailsExtensions.CreateProblemDetails(
                context,
                StatusCodes.Status404NotFound,
                "Resource not found.",
                ex.Message,
                "resource_not_found");
            await WriteProblemDetailsAsync(context, problemDetails);
        }
        catch (BusinessException ex)
        {
            var problemDetails = ProblemDetailsExtensions.CreateProblemDetails(
                context,
                StatusCodes.Status422UnprocessableEntity,
                "Business rule violation.",
                ex.Message,
                "business_rule_violation");
            await WriteProblemDetailsAsync(context, problemDetails);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(error => string.IsNullOrWhiteSpace(error.PropertyName) ? "request" : error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());

            var problemDetails = ProblemDetailsExtensions.CreateValidationProblemDetails(context, errors);
            await WriteProblemDetailsAsync(context, problemDetails);
        }
        catch (Exception ex)
        {
            var problemDetails = ProblemDetailsExtensions.CreateProblemDetails(
                context,
                StatusCodes.Status500InternalServerError,
                "Unexpected error.",
                "An unexpected error occurred.",
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
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
