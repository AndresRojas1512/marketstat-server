using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("MarketStat")
    ?? throw new InvalidOperationException("Missing connection string.");

builder.Services
    .AddDbContext<MarketStatDbContext>(opts => opts.UseNpgsql(conn));

builder.Services
    .AddScoped<IDbContextFactory, NpgsqlDbContextFactory>();

builder.Services
    .AddScoped<IDimEmployerRepository, DimEmployerRepository>()
    .AddScoped<IDimEmployerService, DimEmployerService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
