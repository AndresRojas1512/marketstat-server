using MarketStat.Middleware;
using Serilog;

namespace MarketStat.Extensions;

public static class PipelineExtensions
{
    public static void ConfigurePipelineLogger(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms User: {User} ClientIP: {ClientIP}";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
                diagnosticContext.Set("User", httpContext.User.Identity?.Name ?? "(anonymous)");
            };
        });
    }

    public static void ConfigurePipelineSwagger(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MarketStat API v1");
                c.RoutePrefix = string.Empty;
                c.ConfigObject.AdditionalItems["persistAuthorization"] = true;
            });
        }
    }

    public static void ConfigurePipelineSecurity(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("AllowAngularClient");
        app.UseAuthentication();
        app.UseAuthorization();
    }

    public static void ConfigureGlobalExceptionHandler(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
