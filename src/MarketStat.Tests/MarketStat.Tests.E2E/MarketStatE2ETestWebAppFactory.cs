using MarketStat.Database.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace MarketStat.Tests.E2E;

public class MarketStatE2ETestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private Respawner _respawner = default!;
    private NpgsqlConnection _connection = default!;
    
    public IHost? KestrelHost { get; private set; }

    private const string BaseUrl = "http://127.0.0.1:5050";

    public MarketStatE2ETestWebAppFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("marketstat_e2e_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        var connectionString = _dbContainer.GetConnectionString();
        _connection = new NpgsqlConnection(connectionString);
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MarketStatDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using (var context = new MarketStatDbContext(options))
        {
            await context.Database.MigrateAsync();
        }
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "marketstat" },
            TablesToIgnore = new Respawn.Graph.Table[]
            {
                new Respawn.Graph.Table("__EFMigrationsHistory", "marketstat")
            }
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureWebHost(webBuilder =>
        {
            webBuilder.UseKestrel();
            webBuilder.UseUrls(BaseUrl);
        });
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:MarketStat", _dbContainer.GetConnectionString() },
                { "ASPNETCORE_ENVIRONMENT", "E2ETesting" }
            });
        });
        var host = base.CreateHost(builder);
        KestrelHost = host;
        host.Start();
        return host;
    }

    public HttpClient CreateRealHttpClient()
    {
        return new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_connection);
    }

    public new async Task DisposeAsync()
    {
        if (KestrelHost != null)
        {
            await KestrelHost.StopAsync();
            KestrelHost.Dispose();
        }
        await _connection.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }
}
