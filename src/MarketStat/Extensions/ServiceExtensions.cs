using System.Globalization;
using System.Security.Claims;
using System.Text;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Account;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Database.Repositories.PostgresRepositories.Account;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Database.Repositories.PostgresRepositories.Facts;
using MarketStat.Services.Auth.AuthService;
using MarketStat.Services.Dimensions;
using MarketStat.Services.Dimensions.DimDateService;
using MarketStat.Services.Dimensions.DimEducationService;
using MarketStat.Services.Dimensions.DimEmployeeService;
using MarketStat.Services.Dimensions.DimEmployerService;
using MarketStat.Services.Dimensions.DimIndustryFieldService;
using MarketStat.Services.Dimensions.DimJobService;
using MarketStat.Services.Facts.FactSalaryService;
using MarketStat.Services.Storage;
using MarketStat.Services.Storage.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

namespace MarketStat.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureLogger(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .CreateBootstrapLogger();

        builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId());
    }

    public static void ConfigureCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngularClient", policy =>
                policy
                    .WithOrigins(configuration.GetValue<string>("AllowedOrigins:AngularClient") ?? "http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });
    }

    public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MarketStatDbContext>((serviceProvider, opts) =>
        {
            var connectionString = configuration.GetConnectionString("MarketStat")
                                   ?? throw new InvalidOperationException("Missing connection string 'MarketStat'.");

            opts.UseNpgsql(connectionString, npgsqlOptionsAction: sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "marketstat");
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
                })
                .UseSnakeCaseNamingConvention();
        });

        // Check if we need the factory wrapper (simplified logic)
        if (services.All(s => s.ServiceType != typeof(IDbContextFactory<MarketStatDbContext>)))
        {
            services.AddScoped<IDbContextFactory, NpgsqlDbContextFactory>();
        }
    }

    public static void ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IDimDateRepository, DimDateRepository>();
        services.AddScoped<IDimLocationRepository, DimLocationRepository>();
        services.AddScoped<IDimEducationRepository, DimEducationRepository>();
        services.AddScoped<IDimEmployeeRepository, DimEmployeeRepository>();
        services.AddScoped<IDimEmployerRepository, DimEmployerRepository>();
        services.AddScoped<IDimIndustryFieldRepository, DimIndustryFieldRepository>();
        services.AddScoped<IDimJobRepository, DimJobRepository>();
        services.AddScoped<IFactSalaryRepository, FactSalaryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
    }

    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddScoped<IDimDateService, DimDateService>();
        services.AddScoped<IDimLocationService, DimLocationService>();
        services.AddScoped<IDimEducationService, DimEducationService>();
        services.AddScoped<IDimEmployeeService, DimEmployeeService>();
        services.AddScoped<IDimEmployerService, DimEmployerService>();
        services.AddScoped<IDimIndustryFieldService, DimIndustryFieldService>();
        services.AddScoped<IDimJobService, DimJobService>();
        services.AddScoped<IFactSalaryService, FactSalaryService>();
        services.AddScoped<IAuthService, AuthService>();

        // Storage Service
        services.Configure<StorageSettings>(configuration.GetSection("Storage"));
        services.AddScoped<IReportStorageService, S3ReportStorageService>();
    }

    public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var jwtKey = configuration["JwtSettings:Key"];
        var jwtIssuer = configuration["JwtSettings:Issuer"];
        var jwtAudience = configuration["JwtSettings:Audience"];

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
        {
            Log.Fatal("FATAL ERROR: JWT Key, Issuer, or Audience is not configured.");
            throw new InvalidOperationException("Critical JWT settings are missing.");
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero,
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = "role",
            };
        });

        services.AddAuthorization();
    }

    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MarketStat API",
                Version = "v1",
                Description = "API for MarketStat salary benchmarking.",
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter JWT with Bearer into field (e.g., 'Bearer <your_token>')",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                },
            });
        });
    }
}
