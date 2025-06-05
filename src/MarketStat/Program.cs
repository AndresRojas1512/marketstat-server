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
using MarketStat.Database.Repositories.MongoRepositories;
using MarketStat.Database.Repositories.MongoRepositories.Account;
using MarketStat.Database.Repositories.MongoRepositories.Dimensions;
using MarketStat.Database.Repositories.MongoRepositories.Facts;
using MarketStat.Database.Repositories.PostgresRepositories.Account;
using MarketStat.Middleware;
using MarketStat.Services.Account.BenchmarkHistoryService;
using MarketStat.Services.Auth.AuthService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

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
    
    string? activeProvider = builder.Configuration.GetValue<string>("DatabaseSettings:ActiveProvider");
    bool enableSensitiveDataLogging = builder.Configuration.GetValue<bool>("DatabaseSettings:EnableSensitiveDataLogging", false);
    Log.Information("Active Database Provider from configuration: {ActiveProvider}", activeProvider ?? "Not Set (Defaulting to PostgreSQL)");

    if (activeProvider?.Equals("MongoDB", StringComparison.OrdinalIgnoreCase) == true)
    {
        Log.Information("Configuring application to use MongoDB data provider.");
        var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB");
        if (string.IsNullOrEmpty(mongoConnectionString))
        {
            Log.Fatal("MongoDB connection string ('MongoDB') is missing from configuration.");
            throw new InvalidOperationException("MongoDB connection string ('MongoDB') is missing from configuration.");
        }
        
        builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnectionString));
        builder.Services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var mongoUrl = MongoUrl.Create(mongoConnectionString);
            var databaseName = mongoUrl.DatabaseName;
            if (string.IsNullOrEmpty(databaseName))
            {
                Log.Fatal("MongoDB database name not specified in the connection string.");
                throw new InvalidOperationException("MongoDB database name not specified in the connection string.");
            }
            Log.Information("Connecting to MongoDB database: {DatabaseName}", databaseName);
            return client.GetDatabase(databaseName);
        });
        
        builder.Services.AddScoped<IDimCityRepository, MongoDimCityRepository>();
        builder.Services.AddScoped<IDimDateRepository, MongoDimDateRepository>();
        builder.Services.AddScoped<IDimEducationLevelRepository, MongoDimEducationLevelRepository>();
        builder.Services.AddScoped<IDimEducationRepository, MongoDimEducationRepository>();
        builder.Services.AddScoped<IDimEmployeeRepository, MongoDimEmployeeRepository>();
        builder.Services.AddScoped<IDimEmployeeEducationRepository, MongoDimEmployeeEducationRepository>();
        builder.Services.AddScoped<IDimEmployerRepository, MongoDimEmployerRepository>();
        builder.Services.AddScoped<IDimEmployerIndustryFieldRepository, MongoDimEmployerIndustryFieldRepository>();
        builder.Services.AddScoped<IDimFederalDistrictRepository, MongoDimFederalDistrictRepository>();
        builder.Services.AddScoped<IDimHierarchyLevelRepository, MongoDimHierarchyLevelRepository>();
        builder.Services.AddScoped<IDimIndustryFieldRepository, MongoDimIndustryFieldRepository>();
        builder.Services.AddScoped<IDimJobRoleRepository, MongoDimJobRoleRepository>();
        builder.Services.AddScoped<IDimOblastRepository, MongoDimOblastRepository>();
        builder.Services.AddScoped<IDimStandardJobRoleRepository, MongoDimStandardJobRoleRepository>();
        builder.Services.AddScoped<IDimStandardJobRoleHierarchyRepository, MongoDimStandardJobRoleHierarchyRepository>();
        builder.Services.AddScoped<IFactSalaryRepository, MongoFactSalaryRepository>();
        builder.Services.AddScoped<IUserRepository, MongoUserRepository>();
        builder.Services.AddScoped<IBenchmarkHistoryRepository, MongoBenchmarkHistoryRepository>();
    }
    else
    {
        Log.Information("Configuring application to use PostgreSQL data provider.");
        var pgConnectionString = builder.Configuration.GetConnectionString("PostgreSQL")
                                 ?? throw new InvalidOperationException("PostgreSQL connection string ('PostgreSQL') is missing from configuration.");
        
        builder.Services.AddDbContext<MarketStatDbContext>(opts =>
        {
            opts.UseNpgsql(pgConnectionString, npgsqlOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
                })
                .UseSnakeCaseNamingConvention();
            if (enableSensitiveDataLogging)
            {
                opts.EnableSensitiveDataLogging();
                Log.Information("Sensitive data logging is ENABLED for EF Core (PostgreSQL).");
            }
        });
        
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
        
        builder.Services.AddScoped<IDimCityRepository, DimCityRepository>();
        builder.Services.AddScoped<IDimDateRepository, DimDateRepository>();
        builder.Services.AddScoped<IDimEducationLevelRepository, DimEducationLevelRepository>();
        builder.Services.AddScoped<IDimEducationRepository, DimEducationRepository>();
        builder.Services.AddScoped<IDimEmployeeRepository, DimEmployeeRepository>();
        builder.Services.AddScoped<IDimEmployeeEducationRepository, DimEmployeeEducationRepository>();
        builder.Services.AddScoped<IDimEmployerRepository, DimEmployerRepository>();
        builder.Services.AddScoped<IDimEmployerIndustryFieldRepository, DimEmployerIndustryFieldRepository>();
        builder.Services.AddScoped<IDimFederalDistrictRepository, DimFederalDistrictRepository>();
        builder.Services.AddScoped<IDimHierarchyLevelRepository, DimHierarchyLevelRepository>();
        builder.Services.AddScoped<IDimIndustryFieldRepository, DimIndustryFieldRepository>();
        builder.Services.AddScoped<IDimJobRoleRepository, DimJobRoleRepository>();
        builder.Services.AddScoped<IDimOblastRepository, DimOblastRepository>();
        builder.Services.AddScoped<IDimStandardJobRoleRepository, DimStandardJobRoleRepository>();
        builder.Services.AddScoped<IDimStandardJobRoleHierarchyRepository, DimStandardJobRoleHierarchyRepository>();
        builder.Services.AddScoped<IFactSalaryRepository, FactSalaryRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IBenchmarkHistoryRepository, BenchmarkHistoryRepository>();
    }
    
    builder.Services.AddScoped<IDimCityService, DimCityService>();
    builder.Services.AddScoped<IDimDateService, DimDateService>();
    builder.Services.AddScoped<IDimEducationLevelService, DimEducationLevelService>();
    builder.Services.AddScoped<IDimEducationService, DimEducationService>();
    builder.Services.AddScoped<IDimEmployeeService, DimEmployeeService>();
    builder.Services.AddScoped<IDimEmployeeEducationService, DimEmployeeEducationService>();
    builder.Services.AddScoped<IDimEmployerService, DimEmployerService>();
    builder.Services.AddScoped<IDimEmployerIndustryFieldService, DimEmployerIndustryFieldService>();
    builder.Services.AddScoped<IDimFederalDistrictService, DimFederalDistrictService>();
    builder.Services.AddScoped<IDimHierarchyLevelService, DimHierarchyLevelService>();
    builder.Services.AddScoped<IDimIndustryFieldService, DimIndustryFieldService>();
    builder.Services.AddScoped<IDimJobRoleService, DimJobRoleService>();
    builder.Services.AddScoped<IDimOblastService, DimOblastService>();
    builder.Services.AddScoped<IDimStandardJobRoleService, DimStandardJobRoleService>();
    builder.Services.AddScoped<IDimStandardJobRoleHierarchyService, DimStandardJobRoleHierarchyService>();
    builder.Services.AddScoped<IFactSalaryService, FactSalaryService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IBenchmarkHistoryService, BenchmarkHistoryService>();
    
    builder.Services.AddControllers();
    builder.Services.AddAutoMapper(typeof(Program).Assembly);
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
                RoleClaimType = "role"
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
            c.ConfigObject.AdditionalItems["persistAuthorization"] = true;
        });
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors("AllowAngularClient");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    
    if (activeProvider?.Equals("MongoDB", StringComparison.OrdinalIgnoreCase) == true)
    {
        Log.Information("MongoDB is active. Ensuring indexes...");
        using (var scope = app.Services.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;
            try
            {
                var cityRepo = serviceProvider.GetRequiredService<IDimCityRepository>();
                if (cityRepo is MarketStat.Database.Repositories.MongoRepositories.Dimensions.MongoDimCityRepository mongoCityRepo)
                {
                    await mongoCityRepo.CreateIndexesAsync();
                }
                var userRepo = serviceProvider.GetRequiredService<IUserRepository>();
                if (userRepo is MarketStat.Database.Repositories.MongoRepositories.Account.MongoUserRepository mongoUserRepo)
                {
                    await mongoUserRepo.CreateIndexesAsync();
                }
                var dateRepo = serviceProvider.GetRequiredService<IDimDateRepository>();
                if (dateRepo is MarketStat.Database.Repositories.MongoRepositories.Dimensions.MongoDimDateRepository mongoDateRepo)
                {
                    await mongoDateRepo.CreateIndexesAsync();
                }
                var educationLevelRepo = serviceProvider.GetRequiredService<IDimEducationLevelRepository>();
                if (educationLevelRepo is MarketStat.Database.Repositories.MongoRepositories.Dimensions.MongoDimEducationLevelRepository mongoEducationLevelRepo)
                {
                    await mongoEducationLevelRepo.CreateIndexesAsync();
                }
                var educationRepo = serviceProvider.GetRequiredService<IDimEducationRepository>();
                if (educationRepo is MarketStat.Database.Repositories.MongoRepositories.Dimensions.MongoDimEducationRepository mongoEducationRepo)
                {
                    await mongoEducationRepo.CreateIndexesAsync();
                }
                var employeeEducationRepo = serviceProvider.GetRequiredService<IDimEmployeeEducationRepository>();
                if (employeeEducationRepo is MarketStat.Database.Repositories.MongoRepositories.Dimensions.MongoDimEmployeeEducationRepository mongoEmployeeEducationRepo)
                {
                    await mongoEmployeeEducationRepo.CreateIndexesAsync();
                }
                var employeeRepo = serviceProvider.GetRequiredService<IDimEmployeeRepository>();
                if (employeeRepo is MarketStat.Database.Repositories.MongoRepositories.Dimensions.MongoDimEmployeeRepository mongoEmployeeRepo)
                {
                    await mongoEmployeeRepo.CreateIndexesAsync();
                }
                var employerIndustryFieldRepo = serviceProvider.GetRequiredService<IDimEmployerIndustryFieldRepository>();
                if (employerIndustryFieldRepo is MarketStat.Database.Repositories.MongoRepositories.Dimensions.MongoDimEmployerIndustryFieldRepository mongoEmployerIndustryFieldRepo)
                {
                    await mongoEmployerIndustryFieldRepo.CreateIndexesAsync();
                }
                var employerRepo = serviceProvider.GetRequiredService<IDimEmployerRepository>();
                if (employerRepo is MarketStat.Database.Repositories.MongoRepositories.Dimensions.MongoDimEmployerRepository mongoEmployerRepo)
                {
                    await mongoEmployerRepo.CreateIndexesAsync();
                }
                var federalDistrictRepo = serviceProvider.GetRequiredService<IDimFederalDistrictRepository>();
                if (federalDistrictRepo is MarketStat.Database.Repositories.MongoRepositories.Dimensions.MongoDimFederalDistrictRepository mongoFederalDistrictRepo)
                {
                    await mongoFederalDistrictRepo.CreateIndexesAsync();
                }
                var hierarchyLevelRepo = serviceProvider.GetRequiredService<IDimHierarchyLevelRepository>();
                if (hierarchyLevelRepo is MarketStat.Database.Repositories.MongoRepositories.Dimensions.MongoDimHierarchyLevelRepository mongoHierarchyLevelRepo)
                {
                    await mongoHierarchyLevelRepo.CreateIndexesAsync();
                }
                var industryFieldRepo = serviceProvider.GetRequiredService<IDimIndustryFieldRepository>();
                if (industryFieldRepo is MarketStat.Database.Repositories.MongoRepositories.Dimensions.MongoDimIndustryFieldRepository mongoIndustryFieldRepo)
                {
                    await mongoIndustryFieldRepo.CreateIndexesAsync();
                }
                var jobRoleRepo = serviceProvider.GetRequiredService<IDimJobRoleRepository>();
                if (jobRoleRepo is MarketStat.Database.Repositories.MongoRepositories.Dimensions.MongoDimJobRoleRepository mongoJobRoleRepo)
                {
                    await mongoJobRoleRepo.CreateIndexesAsync();
                }
                var oblastRepo = serviceProvider.GetRequiredService<IDimOblastRepository>();
                if (oblastRepo is MarketStat.Database.Repositories.MongoRepositories.Dimensions.MongoDimOblastRepository mongoOblastRepo)
                {
                    await mongoOblastRepo.CreateIndexesAsync();
                }
                var standardJobRoleHierarchyRepo = serviceProvider.GetRequiredService<IDimStandardJobRoleHierarchyRepository>();
                if (standardJobRoleHierarchyRepo is MarketStat.Database.Repositories.MongoRepositories.Dimensions.MongoDimStandardJobRoleHierarchyRepository mongoStandardJobRoleHierarchyRepo)
                {
                    await mongoStandardJobRoleHierarchyRepo.CreateIndexesAsync();
                }
                var standardJobRoleRepo = serviceProvider.GetRequiredService<IDimStandardJobRoleRepository>();
                if (standardJobRoleRepo is MarketStat.Database.Repositories.MongoRepositories.Dimensions.MongoDimStandardJobRoleRepository mongoStandardJobRoleRepo)
                {
                    await mongoStandardJobRoleRepo.CreateIndexesAsync();
                }
                var benchmarkHistoryRepo = serviceProvider.GetRequiredService<IBenchmarkHistoryRepository>();
                if (benchmarkHistoryRepo is MarketStat.Database.Repositories.MongoRepositories.Account.MongoBenchmarkHistoryRepository mongoBenchmarkHistoryRepo)
                {
                    await mongoBenchmarkHistoryRepo.CreateIndexesAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while ensuring MongoDB indexes.");
            }
        }
    }

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
