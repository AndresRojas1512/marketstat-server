using MarketStat.Data.Consumers.Auth;
using MarketStat.Data.Consumers.Dimensions;
using MarketStat.Data.Consumers.Dimensions.DimDate;
using MarketStat.Data.Consumers.Dimensions.DimEducation;
using MarketStat.Data.Consumers.Dimensions.DimEmployee;
using MarketStat.Data.Consumers.Dimensions.DimEmployer;
using MarketStat.Data.Consumers.Dimensions.DimIndustryField;
using MarketStat.Data.Consumers.Dimensions.DimJob;
using MarketStat.Data.Consumers.Dimensions.DimLocation;
using MarketStat.Data.Consumers.Facts;
using MarketStat.Data.Consumers.Facts.Analytics;
using MarketStat.Data.Services;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Account;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Database.Repositories.PostgresRepositories.Account;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Database.Repositories.PostgresRepositories.Facts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "MarketStat.Data")
    .WriteTo.Console()
    .WriteTo.GrafanaLoki(
        uri: "http://loki:3100", 
        labels: new[] { new LokiLabel { Key = "service", Value = "data" } }
    )
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting MarketStat Data Service ...");
    IHost host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((hostContext, services) =>
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService("MarketStat.Data"))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddMassTransitInstrumentation()
                        .AddSource("MarketStat.Data")
                        .AddOtlpExporter();
                });
            
            services.AddSingleton<IConfiguration>(hostContext.Configuration);
            var connString = hostContext.Configuration.GetConnectionString("MarketStat");
            services.AddDbContext<MarketStatDbContext>(opts =>
                opts.UseNpgsql(connString, o => o.EnableRetryOnFailure())
                    .UseSnakeCaseNamingConvention());
            
            services.AddScoped<IFactSalaryRepository, FactSalaryRepository>();
            services.AddScoped<IDimLocationRepository, DimLocationRepository>();
            services.AddScoped<IDimJobRepository, DimJobRepository>();
            services.AddScoped<IDimIndustryFieldRepository, DimIndustryFieldRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IDimDateRepository, DimDateRepository>();
            services.AddScoped<IDimEducationRepository, DimEducationRepository>();
            services.AddScoped<IDimEmployeeRepository, DimEmployeeRepository>();
            services.AddScoped<IDimEmployerRepository, DimEmployerRepository>();

            services.AddScoped<FilterResolver>();
            services.AddAutoMapper(typeof(Program));
            
            services.AddMassTransit(x =>
            {
                x.AddConsumer<FactSalaryDataConsumer>();
                x.AddConsumer<GetFactSalaryConsumer>();
                x.AddConsumer<FactSalaryAnalyticsConsumer>();
                
                x.AddConsumer<AuthDataConsumer>();
                
                x.AddConsumer<DimDateDataConsumer>();
                x.AddConsumer<DimDateReadConsumer>();

                x.AddConsumer<DimEducationDataConsumer>();
                x.AddConsumer<DimEducationReadConsumer>();

                x.AddConsumer<DimEmployeeDataConsumer>();
                x.AddConsumer<DimEmployeeReadConsumer>();
                
                x.AddConsumer<DimEmployerDataConsumer>();
                x.AddConsumer<DimEmployerReadConsumer>();

                x.AddConsumer<DimIndustryFieldDataConsumer>();
                x.AddConsumer<DimIndustryFieldReadConsumer>();

                x.AddConsumer<DimJobDataConsumer>();
                x.AddConsumer<DimJobReadConsumer>();

                x.AddConsumer<DimLocationDataConsumer>();
                x.AddConsumer<DimLocationReadConsumer>();
                
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("rabbitmq", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                    
                    cfg.ReceiveEndpoint("market-stat-data-writes", e =>
                    {
                        e.ConfigureConsumer<FactSalaryDataConsumer>(context);
                        e.ConfigureConsumer<DimDateDataConsumer>(context);
                        e.ConfigureConsumer<DimEducationDataConsumer>(context);
                        e.ConfigureConsumer<DimEmployeeDataConsumer>(context);
                        e.ConfigureConsumer<DimEmployerDataConsumer>(context);
                        e.ConfigureConsumer<DimIndustryFieldDataConsumer>(context);
                        e.ConfigureConsumer<DimJobDataConsumer>(context);
                        e.ConfigureConsumer<DimLocationDataConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("market-stat-data-reads", e =>
                    {
                        e.ConfigureConsumer<GetFactSalaryConsumer>(context);
                        e.ConfigureConsumer<FactSalaryAnalyticsConsumer>(context);
                        e.ConfigureConsumer<DimDateReadConsumer>(context);
                        e.ConfigureConsumer<DimEducationReadConsumer>(context);
                        e.ConfigureConsumer<DimEmployeeReadConsumer>(context);
                        e.ConfigureConsumer<DimEmployerReadConsumer>(context);
                        e.ConfigureConsumer<DimIndustryFieldReadConsumer>(context);
                        e.ConfigureConsumer<DimJobReadConsumer>(context);
                        e.ConfigureConsumer<DimLocationReadConsumer>(context);
                    });
                    
                    cfg.ReceiveEndpoint("market-stat-data-auth", e => {
                        e.ConfigureConsumer<AuthDataConsumer>(context);
                    });
                });
            });
        })
        .Build();

    var configuration = host.Services.GetRequiredService<IConfiguration>();
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    var runMigrations = Convert.ToBoolean(configuration["RunMigrations"] ?? "false");
    if (runMigrations)
    {
        using (var scope = host.Services.CreateScope())
        {
            try
            {
                logger.LogInformation("[Migration] Starting database migration checks...");
                var defaultConnectionString = configuration.GetConnectionString("MarketStat");
                var adminCb = new NpgsqlConnectionStringBuilder(defaultConnectionString)
                {
                    Username = "marketstat_administrator",
                    Password = "andresrmlnx15",
                    IncludeErrorDetail = true
                };
                var adminOptions = new DbContextOptionsBuilder<MarketStatDbContext>()
                    .UseNpgsql(adminCb.ConnectionString, sqlOpts =>
                    {
                        sqlOpts.MigrationsHistoryTable("__EFMigrationsHistory", "marketstat");
                        sqlOpts.CommandTimeout(60);
                    })
                    .UseSnakeCaseNamingConvention()
                    .Options;

                using var adminContext = new MarketStatDbContext(adminOptions);
                if ((await adminContext.Database.GetPendingMigrationsAsync()).Any())
                {
                    logger.LogInformation("[Migration] Pending migrations found. Applying...");
                    await adminContext.Database.MigrateAsync();
                    logger.LogInformation("[Migration] Database schema successfully applied.");
                }
                else
                {
                    logger.LogInformation("[Migration] Database schema is already up to date.");
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "[Migration] FATAL: Database migration failed.");
            }
        }
    }

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Data Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}