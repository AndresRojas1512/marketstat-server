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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});

builder.WebHost.UseUrls("http://localhost:5000");

var conn = builder.Configuration.GetConnectionString("MarketStat")
    ?? throw new InvalidOperationException("Missing connection string.");

builder.Services
    .AddDbContext<MarketStatDbContext>(opts => opts.UseNpgsql(conn));

builder.Services
    .AddScoped<IDbContextFactory, NpgsqlDbContextFactory>();

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

builder.Services.AddControllers();

builder.Services
    .AddAutoMapper(typeof(MarketStat.MappingProfiles.Dimensions.DimEmployerProfile).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MarketStat API",
        Version = "v1"
    });
});

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.MapGet("/", () => "Hello World!");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MarketStat API v1");
    });
}

app.UseCors("AllowAngularClient");

app.UseAuthorization();
app.MapControllers();
app.Run();
