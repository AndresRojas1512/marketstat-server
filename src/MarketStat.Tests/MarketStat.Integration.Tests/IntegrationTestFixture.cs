using MarketStat.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Respawn;
using Npgsql;
using System;
using System.IO;
using System.Threading.Tasks;
using MarketStat.Database.Models;
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
            await SeedStaticDimensionsAsync(context);
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
                new Respawn.Graph.Table("dim_date", "marketstat"),
                new Respawn.Graph.Table("dim_location", "marketstat"),
                new Respawn.Graph.Table("dim_industry_field", "marketstat"),
                new Respawn.Graph.Table("dim_education", "marketstat"),
                new Respawn.Graph.Table("dim_job", "marketstat"),
                new Respawn.Graph.Table("dim_employer", "marketstat"),
                new Respawn.Graph.Table("dim_employee", "marketstat"),
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
    
    private async Task SeedStaticDimensionsAsync(MarketStatDbContext context)
    {
        if (await context.DimDates.AnyAsync()) 
            return;

        await using var transaction = await context.Database.BeginTransactionAsync();

        context.DimDates.AddRange(
            new DimDateDbModel { DateId = 1, FullDate = new DateOnly(2024, 1, 1), Year = 2024, Quarter = 1, Month = 1 },
            new DimDateDbModel { DateId = 2, FullDate = new DateOnly(2024, 2, 1), Year = 2024, Quarter = 1, Month = 2 },
            new DimDateDbModel { DateId = 3, FullDate = new DateOnly(2024, 5, 1), Year = 2024, Quarter = 2, Month = 5 },
            new DimDateDbModel { DateId = 4, FullDate = new DateOnly(2024, 8, 1), Year = 2024, Quarter = 3, Month = 8 },
            new DimDateDbModel { DateId = 5, FullDate = new DateOnly(2019, 1, 1), Year = 2019, Quarter = 1, Month = 1 }
        );
        await context.SaveChangesAsync();

        context.DimLocations.AddRange(
            new DimLocationDbModel { LocationId = 1, CityName = "Moscow", OblastName = "Moscow", DistrictName = "Central" },
            new DimLocationDbModel { LocationId = 2, CityName = "Tula", OblastName = "Tula", DistrictName = "Central" }
        );
        await context.SaveChangesAsync();

        context.DimIndustryFields.AddRange(
            new DimIndustryFieldDbModel { IndustryFieldId = 1, IndustryFieldName = "IT", IndustryFieldCode = "A.01" },
            new DimIndustryFieldDbModel { IndustryFieldId = 2, IndustryFieldName = "Finance", IndustryFieldCode = "B.02" }
        );
        await context.SaveChangesAsync();

        if (!await context.DimEducations.AnyAsync()) {
             context.DimEducations.Add(new DimEducationDbModel { EducationId = 1, SpecialtyName="CS", SpecialtyCode="01", EducationLevelName="Bach" });
             await context.SaveChangesAsync();
        }

        context.DimJobs.AddRange(
            new DimJobDbModel { JobId = 1, StandardJobRoleTitle = "Engineer", HierarchyLevelName = "Mid", IndustryFieldId = 1 },
            new DimJobDbModel { JobId = 2, StandardJobRoleTitle = "Analyst", HierarchyLevelName = "Senior", IndustryFieldId = 2 }
        );
        await context.SaveChangesAsync();

        context.DimEmployers.Add(new DimEmployerDbModel 
        { 
            EmployerId = 1, 
            EmployerName = "Tech Corp", 
            Inn = "1234567890", Ogrn = "1234567890123", Kpp = "123456789", 
            RegistrationDate = new DateOnly(2000,1,1), LegalAddress = "Addr", ContactEmail = "e@mail.com", ContactPhone = "123",
            IndustryFieldId = 1 
        });
        await context.SaveChangesAsync();

        context.DimEmployees.Add(new DimEmployeeDbModel
        {
            EmployeeId = 1,
            EmployeeRefId = "emp-1",
            BirthDate = new DateOnly(1990, 1, 1),
            CareerStartDate = new DateOnly(2015, 1, 1),
            EducationId = 1
        });
        await context.SaveChangesAsync();

        await transaction.CommitAsync();
    }
}