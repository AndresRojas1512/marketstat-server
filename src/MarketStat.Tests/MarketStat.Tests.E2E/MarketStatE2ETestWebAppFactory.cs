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
    
    // We expose the Host publicly so tests can access Services (Scoped Factories)
    public IHost? KestrelHost { get; private set; }

    // CRITICAL: Matches the port defined in entrypoint.sh for TShark capture
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
        // 1. Start Container
        await _dbContainer.StartAsync();
        var connectionString = _dbContainer.GetConnectionString();
        _connection = new NpgsqlConnection(connectionString);
        await _connection.OpenAsync();
        
        // 2. Apply Migrations & Initial Seed
        var options = new DbContextOptionsBuilder<MarketStatDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using (var context = new MarketStatDbContext(options))
        {
            await context.Database.MigrateAsync();
            await SeedStaticDimensionsAsync(context);
        }

        // 3. Configure Respawner
        // We attempt to ignore dimensions, but ResetDatabaseAsync has a fallback re-seed just in case.
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "marketstat" },
            TablesToIgnore = new Respawn.Graph.Table[]
            {
                new Respawn.Graph.Table("__EFMigrationsHistory", "marketstat"),
                new Respawn.Graph.Table("dim_date", "marketstat"),
                new Respawn.Graph.Table("dim_location", "marketstat"),
                new Respawn.Graph.Table("dim_industry_field", "marketstat"),
                new Respawn.Graph.Table("dim_education", "marketstat"),
                new Respawn.Graph.Table("dim_job", "marketstat"),
                new Respawn.Graph.Table("dim_employer", "marketstat"),
                new Respawn.Graph.Table("dim_employee", "marketstat")
            }
        });

        // 4. Manually Start Kestrel Host
        // We do this manually to bypass WebApplicationFactory's default TestServer logic
        // which conflicts with traffic capture.
        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Program>();
                webBuilder.UseKestrel();
                webBuilder.UseUrls(BaseUrl);
                
                webBuilder.UseEnvironment("E2ETesting");
                
                webBuilder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        { "ConnectionStrings:MarketStat", connectionString }
                    });
                });
            });

        KestrelHost = builder.Build();
        await KestrelHost.StartAsync();
    }

    // Helper for tests to create a real network client
    public HttpClient CreateRealHttpClient()
    {
        return new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };
    }

    public async Task ResetDatabaseAsync()
    {
        // 1. Wipe data (Fact tables)
        await _respawner.ResetAsync(_connection);

        // 2. SELF-HEALING: Ensure dimensions exist
        // This guarantees tests never fail due to Respawner over-deleting
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
    
    // Centralized Seeding Logic for E2E scenarios
    private async Task SeedStaticDimensionsAsync(MarketStatDbContext context)
    {
        // Optimization: If dates exist, assume seeding is already done.
        if (await context.DimDates.AnyAsync()) 
            return;

        await using var transaction = await context.Database.BeginTransactionAsync();

        // 1. Dates
        context.DimDates.AddRange(
            new DimDateDbModel { DateId = 1, FullDate = new DateOnly(2024, 1, 1), Year = 2024, Quarter = 1, Month = 1 },
            new DimDateDbModel { DateId = 5, FullDate = new DateOnly(2019, 1, 1), Year = 2019, Quarter = 1, Month = 1 }
        );
        await context.SaveChangesAsync();

        // 2. Locations
        context.DimLocations.Add(new DimLocationDbModel { LocationId = 1, CityName = "Moscow", OblastName = "Moscow", DistrictName = "Central" });
        await context.SaveChangesAsync();

        // 3. Industry
        context.DimIndustryFields.Add(new DimIndustryFieldDbModel { IndustryFieldId = 1, IndustryFieldName = "IT", IndustryFieldCode = "A.01" });
        await context.SaveChangesAsync();

        // 4. Education
        context.DimEducations.Add(new DimEducationDbModel { EducationId = 1, SpecialtyName="CS", SpecialtyCode="01", EducationLevelName="Bach" });
        await context.SaveChangesAsync();

        // 5. Jobs (Dependent on Industry)
        context.DimJobs.AddRange(
            new DimJobDbModel { JobId = 1, StandardJobRoleTitle = "Senior Architect", HierarchyLevelName = "Senior", IndustryFieldId = 1 },
            new DimJobDbModel { JobId = 2, StandardJobRoleTitle = "Junior Support", HierarchyLevelName = "Junior", IndustryFieldId = 1 },
            new DimJobDbModel { JobId = 3, StandardJobRoleTitle = "Rare Specialist", HierarchyLevelName = "Senior", IndustryFieldId = 1 }
        );
        await context.SaveChangesAsync();

        // 6. Employers (Dependent on Industry)
        context.DimEmployers.Add(new DimEmployerDbModel { EmployerId = 1, EmployerName = "Tech Corp", Inn = "1234567890", Ogrn = "1234567890123", Kpp = "123456789", RegistrationDate = new DateOnly(2000,1,1), LegalAddress = "Addr", ContactEmail = "e@mail.com", ContactPhone = "123", IndustryFieldId = 1 });
        await context.SaveChangesAsync();
        
        // 7. Employees (Dependent on Education)
        context.DimEmployees.Add(new DimEmployeeDbModel { EmployeeId = 1, EmployeeRefId = "emp-1", BirthDate = new DateOnly(1990, 1, 1), CareerStartDate = new DateOnly(2015, 1, 1), EducationId = 1 });
        await context.SaveChangesAsync();

        await transaction.CommitAsync();
    }
}