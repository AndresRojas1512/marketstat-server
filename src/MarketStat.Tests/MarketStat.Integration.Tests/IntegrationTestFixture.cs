using MarketStat.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Respawn;
using Npgsql;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MarketStat.Integration.Tests;

public class IntegrationTestFixture : IAsyncLifetime
{
    public string AdminConnectionString { get; }
    private readonly DbContextOptions<MarketStatDbContext> _dbContextOptions;
    private Respawner _respawner = null!;

    public IntegrationTestFixture()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Integration.json")
            .AddEnvironmentVariables()
            .Build();

        AdminConnectionString = config.GetConnectionString("Admin")
                                ?? throw new InvalidOperationException(
                                    "Could not find 'ConnectionStrings:Admin' in appsettings.Integration.json");

        _dbContextOptions = new DbContextOptionsBuilder<MarketStatDbContext>()
            .UseNpgsql(AdminConnectionString, o => 
                o.MigrationsHistoryTable("__EFMigrationsHistory", "marketstat"))
            .UseSnakeCaseNamingConvention()
            .Options;
    }

    public async Task InitializeAsync()
    {
        await using (var context = new MarketStatDbContext(_dbContextOptions))
        {
            await context.Database.MigrateAsync();
        }
        await using var conn = new NpgsqlConnection(AdminConnectionString);
        await conn.OpenAsync();

        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "marketstat", "public" },
            TablesToIgnore = new Respawn.Graph.Table[] 
            {
                new Respawn.Graph.Table("__EFMigrationsHistory", "marketstat"),
                new Respawn.Graph.Table("__EFMigrationsHistory", "public")
            }
        });
    }
    
    public async Task DisposeAsync()
    {
        if (_respawner == null)
        {
            return;
        }
        await using var conn = new NpgsqlConnection(AdminConnectionString);
        await conn.OpenAsync();
        await _respawner.ResetAsync(conn);
    }

    public MarketStatDbContext CreateContext()
    {
        return new MarketStatDbContext(_dbContextOptions);
    }

    public async Task ResetDatabaseAsync()
    {
        await using var conn = new NpgsqlConnection(AdminConnectionString);
        await conn.OpenAsync();
        await _respawner.ResetAsync(conn);
    }
}