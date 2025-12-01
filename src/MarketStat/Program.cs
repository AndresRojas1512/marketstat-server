using System.Globalization;
using MarketStat.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("--- MarketStat API: Starting host builder ---");

    var builder = WebApplication.CreateBuilder(args);

    // 1. Configure Services (Dependency Injection)
    // All logic is inside ServiceExtensions.cs
    builder.ConfigureLogger(); // Configures Serilog from settings
    builder.Services.ConfigureCors(builder.Configuration);
    builder.Services.ConfigureDatabase(builder.Configuration);
    builder.Services.ConfigureRepositories();
    builder.Services.ConfigureServices(builder.Configuration);
    builder.Services.ConfigureAuthentication(builder.Configuration);
    builder.Services.ConfigureSwagger();

    builder.Services.AddControllers();
    builder.Services.AddAutoMapper(typeof(Program).Assembly);

    var app = builder.Build();
    app.ConfigurePipelineLogger();
    app.ConfigureGlobalExceptionHandler();
    app.ConfigurePipelineSwagger();
    app.ConfigurePipelineSecurity();

    app.MapControllers();

    Log.Information("--- MarketStat API: Host built, starting application ---");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "--- MarketStat API: Host terminated unexpectedly ---");
    throw;
}
finally
{
    Log.Information("--- MarketStat API: Shutting down ---");
    Log.CloseAndFlush();
}

public partial class Program
{
}
