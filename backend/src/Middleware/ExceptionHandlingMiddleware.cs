using backend.src.Common.Exceptions;
using backend.src.Common.Models;
using System.Net;
using System.Text.Json;

namespace backend.src.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Business rule violated: {Rule}", ex.Rule);
            await HandleBusinessRuleExceptionAsync(context, ex);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            await HandleNotFoundExceptionAsync(context, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access");
            await HandleUnauthorizedExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleGenericExceptionAsync(context, ex);
        }
    }

    private static Task HandleBusinessRuleExceptionAsync(HttpContext context, BusinessRuleException exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Type = "business_rule_error",
            Message = exception.Message,
            Rule = exception.Rule,
            StatusCode = (int)HttpStatusCode.BadRequest
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static Task HandleNotFoundExceptionAsync(HttpContext context, KeyNotFoundException exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Type = "not_found",
            Message = exception.Message,
            StatusCode = (int)HttpStatusCode.NotFound
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static Task HandleUnauthorizedExceptionAsync(HttpContext context, UnauthorizedAccessException exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Type = "unauthorized",
            Message = exception.Message,
            StatusCode = (int)HttpStatusCode.Unauthorized
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static Task HandleGenericExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var isDevelopment = context.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() ?? false;

        var response = new ErrorResponse
        {
            Type = "internal_error",
            Message = isDevelopment ? exception.Message : "An unexpected error occurred.",
            StatusCode = (int)HttpStatusCode.InternalServerError
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
