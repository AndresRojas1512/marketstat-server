using MarketStat.Database.Context;
using MarketStat.Database.Models;
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
            await SeedStaticDimensionsAsync(context);
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
        return new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_connection);
        var options = new DbContextOptionsBuilder<MarketStatDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var context = new MarketStatDbContext(options);
        await SeedStaticDimensionsAsync(context);
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
    
    private async Task SeedStaticDimensionsAsync(MarketStatDbContext context)
    {
        if (await context.DimDates.AnyAsync()) return;
        await using var transaction = await context.Database.BeginTransactionAsync();
        context.DimDates.AddRange(
            new DimDateDbModel { DateId = 1, FullDate = new DateOnly(2024, 1, 1), Year = 2024, Quarter = 1, Month = 1 },
            new DimDateDbModel { DateId = 5, FullDate = new DateOnly(2019, 1, 1), Year = 2019, Quarter = 1, Month = 1 }
        );
        await context.SaveChangesAsync();

        context.DimLocations.Add(new DimLocationDbModel { LocationId = 1, CityName = "Moscow", OblastName = "Moscow", DistrictName = "Central" });
        await context.SaveChangesAsync();

        context.DimIndustryFields.Add(new DimIndustryFieldDbModel { IndustryFieldId = 1, IndustryFieldName = "IT", IndustryFieldCode = "A.01" });
        await context.SaveChangesAsync();

        context.DimEducations.Add(new DimEducationDbModel { EducationId = 1, SpecialtyName="CS", SpecialtyCode="01", EducationLevelName="Bach" });
        await context.SaveChangesAsync();

        context.DimJobs.AddRange(
            new DimJobDbModel { JobId = 1, StandardJobRoleTitle = "Senior Architect", HierarchyLevelName = "Senior", IndustryFieldId = 1 },
            new DimJobDbModel { JobId = 2, StandardJobRoleTitle = "Junior Support", HierarchyLevelName = "Junior", IndustryFieldId = 1 },
            new DimJobDbModel { JobId = 3, StandardJobRoleTitle = "Rare Specialist", HierarchyLevelName = "Senior", IndustryFieldId = 1 }
        );
        await context.SaveChangesAsync();

        context.DimEmployers.Add(new DimEmployerDbModel { EmployerId = 1, EmployerName = "Tech Corp", Inn = "1234567890", Ogrn = "1234567890123", Kpp = "123456789", RegistrationDate = new DateOnly(2000,1,1), LegalAddress = "Addr", ContactEmail = "e@mail.com", ContactPhone = "123", IndustryFieldId = 1 });
        await context.SaveChangesAsync();
        
        context.DimEmployees.Add(new DimEmployeeDbModel { EmployeeId = 1, EmployeeRefId = "emp-1", BirthDate = new DateOnly(1990, 1, 1), CareerStartDate = new DateOnly(2015, 1, 1), EducationId = 1 });
        await context.SaveChangesAsync();

        await transaction.CommitAsync();
    }
}