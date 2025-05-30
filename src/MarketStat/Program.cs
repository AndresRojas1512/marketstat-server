using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Database.Models;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Database.Repositories.PostgresRepositories.Facts;
using MarketStat.Services.Dimensions.DimCityService;
using MarketStat.Services.Dimensions.DimDateService;
using MarketStat.Services.Dimensions.DimEducationLevelService;
using MarketStat.Services.Dimensions.DimEducationService;
using MarketStat.Services.Dimensions.DimEmployeeEducationService;
using MarketStat.Services.Dimensions.DimEmployeeService;
using MarketStat.Services.Dimensions.DimEmployerIndustryFieldService;
using MarketStat.Services.Dimensions.DimEmployerService;
using MarketStat.Services.Dimensions.DimFederalDistrictService;
using MarketStat.Services.Dimensions.DimHierarchyLevelService;
using MarketStat.Services.Dimensions.DimIndustryFieldService;
using MarketStat.Services.Dimensions.DimJobRoleService;
using MarketStat.Services.Dimensions.DimOblastService;
using MarketStat.Services.Dimensions.DimStandardJobRoleHierarchyService;
using MarketStat.Services.Dimensions.DimStandardJobRoleService;
using MarketStat.Services.Facts.FactSalaryService;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using MarketStat.Database.Core.Repositories.Account;
using MarketStat.Database.Repositories.PostgresRepositories.Account;
using MarketStat.Middleware;
using MarketStat.Services.Account.BenchmarkHistoryService;
using MarketStat.Services.Auth.AuthService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Microsoft.EntityFrameworkCore;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning) // Keep EF Core commands quieter during bootstrap
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("--- MarketStat API: Starting host builder ---");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
    );

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularClient", policy =>
            policy
                .WithOrigins(builder.Configuration.GetValue<string>("AllowedOrigins:AngularClient") ?? "http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
        );
    });

    var connectionString = builder.Configuration.GetConnectionString("MarketStat")
        ?? throw new InvalidOperationException("Missing connection string 'MarketStat'.");

    builder.Services.AddDbContext<MarketStatDbContext>(opts =>
        opts.UseNpgsql(connectionString, npgsqlOptionsAction: sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
            })
        .UseSnakeCaseNamingConvention()
    );
    
    if (builder.Services.All(s => s.ServiceType != typeof(IDbContextFactory<MarketStatDbContext>)))
    {
        if (typeof(NpgsqlDbContextFactory).IsAssignableTo(typeof(IDbContextFactory)))
        {
             builder.Services.AddScoped<IDbContextFactory, NpgsqlDbContextFactory>();
             Log.Information("Registered custom NpgsqlDbContextFactory for non-generic IDbContextFactory.");
        }
        else
        {
            Log.Warning("NpgsqlDbContextFactory not registered as IDbContextFactory as it does not implement the non-generic interface, or type mismatch.");
        }
    }


    builder.Services
        .AddScoped<IDimCityRepository, DimCityRepository>()
        .AddScoped<IDimCityService, DimCityService>()
        .AddScoped<IDimDateRepository, DimDateRepository>()
        .AddScoped<IDimDateService, DimDateService>()
        .AddScoped<IDimEducationLevelRepository, DimEducationLevelRepository>()
        .AddScoped<IDimEducationLevelService, DimEducationLevelService>()
        .AddScoped<IDimEducationRepository, DimEducationRepository>()
        .AddScoped<IDimEducationService, DimEducationService>()
        .AddScoped<IDimEmployeeRepository, DimEmployeeRepository>()
        .AddScoped<IDimEmployeeService, DimEmployeeService>()
        .AddScoped<IDimEmployeeEducationRepository, DimEmployeeEducationRepository>()
        .AddScoped<IDimEmployeeEducationService, DimEmployeeEducationService>()
        .AddScoped<IDimEmployerRepository, DimEmployerRepository>()
        .AddScoped<IDimEmployerService, DimEmployerService>()
        .AddScoped<IDimEmployerIndustryFieldRepository, DimEmployerIndustryFieldRepository>()
        .AddScoped<IDimEmployerIndustryFieldService, DimEmployerIndustryFieldService>()
        .AddScoped<IDimFederalDistrictRepository, DimFederalDistrictRepository>()
        .AddScoped<IDimFederalDistrictService, DimFederalDistrictService>()
        .AddScoped<IDimHierarchyLevelRepository, DimHierarchyLevelRepository>()
        .AddScoped<IDimHierarchyLevelService, DimHierarchyLevelService>()
        .AddScoped<IDimIndustryFieldRepository, DimIndustryFieldRepository>()
        .AddScoped<IDimIndustryFieldService, DimIndustryFieldService>()
        .AddScoped<IDimJobRoleRepository, DimJobRoleRepository>()
        .AddScoped<IDimJobRoleService, DimJobRoleService>()
        .AddScoped<IDimOblastRepository, DimOblastRepository>()
        .AddScoped<IDimOblastService, DimOblastService>()
        .AddScoped<IDimStandardJobRoleRepository, DimStandardJobRoleRepository>()
        .AddScoped<IDimStandardJobRoleService, DimStandardJobRoleService>()
        .AddScoped<IDimStandardJobRoleHierarchyRepository, DimStandardJobRoleHierarchyRepository>()
        .AddScoped<IDimStandardJobRoleHierarchyService, DimStandardJobRoleHierarchyService>();

    builder.Services
        .AddScoped<IFactSalaryRepository, FactSalaryRepository>()
        .AddScoped<IFactSalaryService, FactSalaryService>();

    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    builder.Services.AddScoped<IBenchmarkHistoryRepository, BenchmarkHistoryRepository>();
    builder.Services.AddScoped<IBenchmarkHistoryService, BenchmarkHistoryService>();

    builder.Services.AddControllers();
    builder.Services.AddAutoMapper(typeof(Program).Assembly); // Scans for profiles in the current assembly
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "MarketStat API",
            Version = "v1",
            Description = "API for MarketStat salary benchmarking."
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter JWT with Bearer into field (e.g., 'Bearer <your_token>')",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }});
    });

    var jwtKey = builder.Configuration["JwtSettings:Key"];
    var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
    var jwtAudience = builder.Configuration["JwtSettings:Audience"];

    if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
    {
        Log.Fatal("FATAL ERROR: JWT Key, Issuer, or Audience is not configured in appsettings.json. Authentication will NOT work correctly. Application will not start.");
        throw new InvalidOperationException("Critical JWT settings are missing from configuration. Application cannot start.");
    }
    else
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false; // Recommended to prevent default claim type mapping
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
                RoleClaimType = ClaimTypes.Role
            };
        });
    }
    builder.Services.AddAuthorization();


    var app = builder.Build();


    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms User: {User} ClientIP: {ClientIP}";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
            // Get User.Identity.Name which should be populated based on NameClaimType in JWT options
            diagnosticContext.Set("User", httpContext.User.Identity?.Name ?? "(anonymous)"); 
        };
    });

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "MarketStat API v1");
            c.RoutePrefix = string.Empty;
            c.ConfigObject.AdditionalItems["persistAuthorization"] = true; // Persist JWT in Swagger UI
        });
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors("AllowAngularClient");

    app.UseAuthentication();
    app.UseAuthorization();

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