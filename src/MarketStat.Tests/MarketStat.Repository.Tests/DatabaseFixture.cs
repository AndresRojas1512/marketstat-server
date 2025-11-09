using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Enums;
using MarketStat.Database.Context;
using MarketStat.Database.Models;
using MarketStat.Database.Repositories.PostgresRepositories.Facts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace MarketStat.Repository.Tests;

public class DatabaseFixture : IDisposable
{
    public readonly DbContextOptions<MarketStatDbContext> Options;

    public DatabaseFixture()
    {
        Options = new DbContextOptionsBuilder<MarketStatDbContext>()
            .UseInMemoryDatabase(databaseName: "MarketStatTestDb")
            .Options;
        using var context = new MarketStatDbContext(Options);
        context.Database.EnsureCreated();
        WarmupEfAndRepositories(context);
    }

    public MarketStatDbContext CreateCleanContext()
    {
        var context = new MarketStatDbContext(Options);
        
        // context.BenchmarkHistories.RemoveRange(context.BenchmarkHistories);
        // context.Users.RemoveRange(context.Users);
        //
        // context.DimDates.RemoveRange(context.DimDates);
        // context.DimEducations.RemoveRange(context.DimEducations);
        // context.DimEmployees.RemoveRange(context.DimEmployees);
        // context.DimEmployers.RemoveRange(context.DimEmployers);
        // context.DimIndustryFields.RemoveRange(context.DimIndustryFields);
        // context.DimJobs.RemoveRange(context.DimJobs);
        // context.DimLocations.RemoveRange(context.DimLocations);
        //
        // context.FactSalaries.RemoveRange(context.FactSalaries);
        
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        return context;
    }

    public void Dispose()
    {
        using var context = new MarketStatDbContext(Options);
        context.Database.EnsureDeleted();
    }
    
    private static void WarmupEfAndRepositories(MarketStatDbContext context)
    {
        if (!context.DimDates.Any())
        {
            context.DimDates.Add(new DimDateDbModel
            {
                DateId = 1,
                FullDate = new DateOnly(2024, 1, 1),
                Year = 2024,
                Quarter = 1,
                Month = 1
            });
            context.DimLocations.Add(new DimLocationDbModel
            {
                LocationId = 1,
                CityName = "WarmupCity",
                OblastName = "WarmupOblast",
                DistrictName = "WarmupDistrict"
            });
            context.DimIndustryFields.Add(new DimIndustryFieldDbModel
            {
                IndustryFieldId = 1,
                IndustryFieldName = "Warmup",
                IndustryFieldCode = "W.00"
            });
            context.DimJobs.Add(new DimJobDbModel
            {
                JobId = 1,
                StandardJobRoleTitle = "WarmupRole",
                HierarchyLevelName = "Mid",
                IndustryFieldId = 1
            });
            context.DimEmployers.Add(new DimEmployerDbModel
            {
                EmployerId = 1,
                EmployerName = "Warmup Corp"
            });
            context.DimEmployees.Add(new DimEmployeeDbModel
            {
                EmployeeId = 1,
                EmployeeRefId = "emp-warmup"
            });

            context.SaveChanges();

            var repo = new FactSalaryRepository(
                context
            );

            var fs = new FactSalary
            {
                SalaryFactId = 0,
                DateId = 1,
                LocationId = 1,
                EmployerId = 1,
                JobId = 1,
                EmployeeId = 1,
                SalaryAmount = 100
            };

            repo.AddFactSalaryAsync(fs).GetAwaiter().GetResult();

            var filter = new ResolvedSalaryFilter
            {
                DateStart = new DateOnly(2024, 1, 1),
                DateEnd = new DateOnly(2024, 12, 31),
                JobIds = new List<int> { 1 },
                LocationIds = new List<int> { 1 }
            };

            repo.GetFactSalariesByFilterAsync(filter).GetAwaiter().GetResult();
            repo.GetSalarySummaryAsync(filter, 90).GetAwaiter().GetResult();
            repo.GetSalaryDistributionAsync(filter).GetAwaiter().GetResult();
            repo.GetSalaryTimeSeriesAsync(filter, TimeGranularity.Month, 3).GetAwaiter().GetResult();
            repo.GetPublicRolesAsync(filter, 1).GetAwaiter().GetResult();

            context.FactSalaries.RemoveRange(context.FactSalaries);
            context.DimDates.RemoveRange(context.DimDates);
            context.DimLocations.RemoveRange(context.DimLocations);
            context.DimJobs.RemoveRange(context.DimJobs);
            context.DimEmployers.RemoveRange(context.DimEmployers);
            context.DimEmployees.RemoveRange(context.DimEmployees);
            context.DimIndustryFields.RemoveRange(context.DimIndustryFields);
            context.SaveChanges();
        }
    }
}
