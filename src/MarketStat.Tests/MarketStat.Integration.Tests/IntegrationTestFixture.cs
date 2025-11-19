using MarketStat.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Respawn;
using Npgsql;
using System;
using System.IO;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Xunit;

namespace MarketStat.Integration.Tests;

public class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;

    public string AdminConnectionString { get; private set; } = null!;
    private DbContextOptions<MarketStatDbContext> _dbContextOptions = null!;
    private Respawner _respawner = null!;

    public IntegrationTestFixture()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("marketstat_it_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        AdminConnectionString = _dbContainer.GetConnectionString();
        _dbContextOptions = new DbContextOptionsBuilder<MarketStatDbContext>()
            .UseNpgsql(AdminConnectionString, o =>
                o.MigrationsHistoryTable("__EFMigrationsHistory", "marketstat"))
            .UseSnakeCaseNamingConvention()
            .Options;

        await using (var context = new MarketStatDbContext(_dbContextOptions))
        {
            await context.Database.MigrateAsync();
        }

        await using var conn = new NpgsqlConnection(AdminConnectionString);
        await conn.OpenAsync();
        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] {"marketstat"},
            TablesToIgnore = new Respawn.Graph.Table[]
            {
                new Respawn.Graph.Table("__EFMigrationsHistory", "marketstat"),
            }
        });
    }
    
    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
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