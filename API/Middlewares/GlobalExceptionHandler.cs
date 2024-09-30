using System.Net;
using System.Text.Json;
using API.Models;
using Infra.Options;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace API.Middlewares;

public class GlobalExceptionHandler(
    IOptions<EnvironmentOptions> environmentOptions) : IExceptionHandler
{
    private readonly EnvironmentOptions _environmentOptions = environmentOptions.Value;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, errorCode, errorDescription) = GetExceptionDetails(exception);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        if (_environmentOptions.IsDevelopment)
        {
            errorDescription = exception.Message;
        }

        var errorResponse = new ErrorResponse
        {
            Errors = [new() { Code = errorCode, Description = errorDescription }]
        };

        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(errorResponse), cancellationToken);

        return true;
    }

    private static (int StatusCode, string ErrorCode, string ErrorDescription) GetExceptionDetails(Exception exception) => exception switch
    {
        UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized", "Unauthorized access"),
        InvalidOperationException => ((int)HttpStatusCode.BadRequest, "InvalidOperation", "Invalid operation"),
        ArgumentException => ((int)HttpStatusCode.BadRequest, "InvalidArgument", "Invalid argument"),
        KeyNotFoundException => ((int)HttpStatusCode.NotFound, "NotFound", "Resource not found"),
        _ => ((int)HttpStatusCode.InternalServerError, "InternalServerError", "An unexpected error occurred")
    };
}