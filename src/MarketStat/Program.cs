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
using System.Text;
using MarketStat.Database.Core.Repositories.Account;
using MarketStat.Database.Repositories.PostgresRepositories.Account;
using MarketStat.Middleware;
using MarketStat.Services.Account.BenchmarkHistoryService;
using MarketStat.Services.Auth.AuthService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

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
    opts.UseNpgsql(connectionString));

if (builder.Services.All(s => s.ServiceType != typeof(IDbContextFactory<MarketStatDbContext>)))
{
    builder.Services.AddScoped<IDbContextFactory, NpgsqlDbContextFactory>();
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
    var tempLogger = LoggerFactory.Create(config => { config.AddConsole(); }).CreateLogger("ProgramCritical");
    tempLogger.LogCritical("FATAL ERROR: JWT Key, Issuer, or Audience is not configured in appsettings.json. Authentication will NOT work correctly.");
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
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
}

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

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

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAngularClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
