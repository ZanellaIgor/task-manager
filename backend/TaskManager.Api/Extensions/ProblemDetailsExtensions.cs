using Microsoft.AspNetCore.Mvc;

namespace TaskManager.Api.Extensions;

public static class ProblemDetailsExtensions
{
    public static ProblemDetails CreateProblemDetails(
        HttpContext context,
        int status,
        string title,
        string detail,
        string? code = null)
    {
        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.com/{status}",
            Instance = context.Request.Path,
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        if (!string.IsNullOrWhiteSpace(code))
        {
            problemDetails.Extensions["code"] = code;
        }

        return problemDetails;
    }

    public static ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext context,
        IDictionary<string, string[]> errors,
        int status = StatusCodes.Status400BadRequest)
    {
        var problemDetails = new ValidationProblemDetails(errors)
        {
            Status = status,
            Title = "Validation failed.",
            Detail = "One or more validation errors occurred.",
            Type = $"https://httpstatuses.com/{status}",
            Instance = context.Request.Path,
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        return problemDetails;
    }
}
