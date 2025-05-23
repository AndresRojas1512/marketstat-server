using System.Net;
using System.Text.Json;
using MarketStat.Common.Exceptions;

namespace MarketStat.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        HttpStatusCode statusCode;
        string message = ex.Message;

        switch (ex)
        {
            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                break;
            case ConflictException conflictEx:
                statusCode = HttpStatusCode.Conflict;
                break;
            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                break;
            case ArgumentException argEx:
                statusCode = HttpStatusCode.BadRequest;
                break;
            case MarketStat.Common.Exceptions.AuthenticationException authEx:
                statusCode = HttpStatusCode.Unauthorized;
                message = authEx.Message;
                break;
            case System.Security.Authentication.AuthenticationException stdAuthEx:
                statusCode = HttpStatusCode.Unauthorized;
                message = stdAuthEx.Message;
                break;
            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = "An unexpected internal server error has occurred.";
                break;
        }

        _logger.LogError(ex, "Exception occurred: {ErrorMessage}. Responding with HTTP {StatusCode}.", ex.Message, (int)statusCode);
        
        var errorResponse = new { error = (statusCode == HttpStatusCode.InternalServerError && !(ex is ApplicationException) ? "An unexpected error occurred." : message) };
        var result = JsonSerializer.Serialize(errorResponse);
        
        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = (int)statusCode;
        return ctx.Response.WriteAsync(result);
    }
}