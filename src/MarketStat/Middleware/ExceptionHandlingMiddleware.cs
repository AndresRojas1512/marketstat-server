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
        var statusCode = ex switch
        {
            NotFoundException=> HttpStatusCode.NotFound,
            ConflictException => HttpStatusCode.Conflict,
            ValidationException => HttpStatusCode.BadRequest,
            ArgumentException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };
        _logger.LogError(ex, "Unhandled exception occurred.");
        var result = JsonSerializer.Serialize(new { error = ex.Message });
        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = (int)statusCode;
        return ctx.Response.WriteAsync(result);
    }
}