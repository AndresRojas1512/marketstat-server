using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerService;
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
    .AddScoped<IDimEmployerRepository, DimEmployerRepository>()
    .AddScoped<IDimEmployerService, DimEmployerService>();

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
