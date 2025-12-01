using System.Diagnostics.CodeAnalysis;

namespace MarketStat.Middleware;

using System.Net;
using System.Text.Json;
using MarketStat.Common.Exceptions;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Global exception handler must catch all exceptions.")]
    public async Task Invoke(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex).ConfigureAwait(false);
        }
    }

    private Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        HttpStatusCode statusCode;
        string clientMessage;

        switch (ex)
        {
            case NotFoundException:
                statusCode = HttpStatusCode.NotFound;
                clientMessage = ex.Message;
                break;
            case ConflictException:
                statusCode = HttpStatusCode.Conflict;
                clientMessage = ex.Message;
                break;
            case ValidationException:
                statusCode = HttpStatusCode.BadRequest;
                clientMessage = ex.Message;
                break;
            case ArgumentException:
                statusCode = HttpStatusCode.BadRequest;
                clientMessage = ex.Message;
                break;
            case MarketStat.Common.Exceptions.AuthenticationException:
                statusCode = HttpStatusCode.Unauthorized;
                clientMessage = ex.Message;
                break;
            case System.Security.Authentication.AuthenticationException:
                statusCode = HttpStatusCode.Unauthorized;
                clientMessage = ex.Message;
                break;
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Forbidden;
                clientMessage = string.IsNullOrEmpty(ex.Message) || ex.Message == "User ID could not be determined or is invalid from the token."
                                ? "You are not authorized to perform this action or your session is invalid."
                                : ex.Message;
                break;
            default:
                statusCode = HttpStatusCode.InternalServerError;
                clientMessage = "An unexpected internal server error has occurred.";
                break;
        }

        _logger.LogError(
            ex,
            "Exception Handled: {ExceptionType} - Message: {OriginalErrorMessage}. Responding with HTTP {StatusCode} and client message: {ClientMessage}",
            ex.GetType().Name,
            ex.Message,
            (int)statusCode,
            clientMessage);

        var finalClientMessage = (statusCode == HttpStatusCode.InternalServerError && !(ex is ApplicationException))
                                 ? "An unexpected internal server error has occurred."
                                 : clientMessage;

        var errorResponse = new { error = finalClientMessage };
        var result = JsonSerializer.Serialize(errorResponse);

        ctx.Response.Clear();
        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = (int)statusCode;
        return ctx.Response.WriteAsync(result);
    }
}
