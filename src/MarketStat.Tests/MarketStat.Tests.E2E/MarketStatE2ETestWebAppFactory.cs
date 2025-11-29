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
    
    // Expose the Host so tests can access Services (Scoped Factories)
    public IHost? KestrelHost { get; private set; }

    // CRITICAL: This must match the port in entrypoint.sh for TShark capture
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
        
        var options = new DbContextOptionsBuilder<MarketStatDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        // 2. Apply Migrations & Initial Seed
        await using (var context = new MarketStatDbContext(options))
        {
            await context.Database.MigrateAsync();
            await SeedStaticDimensionsAsync(context);
        }

        // 3. Configure Respawner
        // NOTE: We remove the explicit schema from TablesToIgnore to ensure better matching
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "marketstat" },
            TablesToIgnore = new Respawn.Graph.Table[]
            {
                new Respawn.Graph.Table("__EFMigrationsHistory"),
                new Respawn.Graph.Table("dim_date"),
                new Respawn.Graph.Table("dim_location"),
                new Respawn.Graph.Table("dim_industry_field"),
                new Respawn.Graph.Table("dim_education"),
                new Respawn.Graph.Table("dim_job"),
                new Respawn.Graph.Table("dim_employer"),
                new Respawn.Graph.Table("dim_employee")
            }
        });

        // 4. Manually Start Kestrel Host
        // We override the host creation here to ensure it binds to the TCP socket
        // required for traffic capture.
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

    protected override IHost CreateHost(IHostBuilder builder) => base.CreateHost(builder);

    public HttpClient CreateRealHttpClient()
    {
        return new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public async Task ResetDatabaseAsync()
    {
        // 1. Wipe data (Should only wipe Fact tables due to Ignore, but we play safe)
        await _respawner.ResetAsync(_connection);

        // 2. SELF-HEALING: "Heal" the database by re-seeding if dimensions were wiped.
        // This prevents the Foreign Key Violation (23503) errors.
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
        var count = await context.DimDates.CountAsync();
        Console.WriteLine($"[SEED DEBUG] DimDates count before seed: {count}");
        
        // Check if data exists. If Respawner worked correctly, this returns true and we exit fast.
        // If Respawner wiped it, this returns false and we re-seed.
        if (await context.DimDates.AnyAsync()) return;
        
        Console.WriteLine("[SEED DEBUG] Seeding dimensions...");
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
        Console.WriteLine("[SEED DEBUG] Seeding complete");
    }
}