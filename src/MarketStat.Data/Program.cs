using MarketStat.Data.Consumers.Auth;
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

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
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

        services.AddScoped<FilterResolver>();
        services.AddAutoMapper(typeof(Program));
        
        services.AddMassTransit(x =>
        {
            x.AddConsumer<FactSalaryDataConsumer>();
            x.AddConsumer<GetFactSalaryConsumer>();
            x.AddConsumer<FactSalaryAnalyticsConsumer>();
            x.AddConsumer<AuthDataConsumer>();
            x.AddConsumer<AuthLoginConsumer>();
            
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
                });

                cfg.ReceiveEndpoint("market-stat-data-reads", e =>
                {
                    e.ConfigureConsumer<GetFactSalaryConsumer>(context);
                    e.ConfigureConsumer<FactSalaryAnalyticsConsumer>(context);
                });
                
                cfg.ReceiveEndpoint("market-stat-data-auth", e => {
                    e.ConfigureConsumer<AuthDataConsumer>(context);
                    e.ConfigureConsumer<AuthLoginConsumer>(context);
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