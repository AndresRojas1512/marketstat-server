using FluentAssertions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Database.Context;
using MarketStat.Database.Models;
using MarketStat.Database.Repositories.PostgresRepositories.Facts;
using MarketStat.Tests.TestData.Builders.Facts;
using Xunit;

namespace MarketStat.Integration.Tests.Facts;

[Collection("Integration")]
public class FactSalaryRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private readonly MarketStatDbContext _dbContext;
    private readonly FactSalaryRepository _sut;

    public FactSalaryRepositoryIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _dbContext = _fixture.CreateContext();
        _sut = new FactSalaryRepository(_dbContext);
    }

    public Task InitializeAsync()
    {
        return SeedDimensionsAsync(_dbContext);
    }

    public Task DisposeAsync()
    {
        return _fixture.ResetDatabaseAsync();
    }
    
    private async Task SeedDimensionsAsync(MarketStatDbContext context)
    {
        context.DimDates.AddRange(
            new DimDateDbModel { DateId = 1, FullDate = new DateOnly(2024, 1, 1), Year = 2024, Quarter = 1, Month = 1 },
            new DimDateDbModel { DateId = 2, FullDate = new DateOnly(2024, 2, 1), Year = 2024, Quarter = 1, Month = 2 },
            new DimDateDbModel { DateId = 3, FullDate = new DateOnly(2024, 5, 1), Year = 2024, Quarter = 2, Month = 5 },
            new DimDateDbModel { DateId = 4, FullDate = new DateOnly(2024, 8, 1), Year = 2024, Quarter = 3, Month = 8 },
            new DimDateDbModel { DateId = 5, FullDate = new DateOnly(2019, 1, 1), Year = 2019, Quarter = 1, Month = 1 }
        );
        
        context.DimLocations.AddRange(
            new DimLocationDbModel { LocationId = 1, CityName = "Moscow", OblastName = "Moscow", DistrictName = "Central" },
            new DimLocationDbModel { LocationId = 2, CityName = "Tula", OblastName = "Tula", DistrictName = "Central" }
        );
        
        context.DimIndustryFields.AddRange(
            new DimIndustryFieldDbModel { IndustryFieldId = 1, IndustryFieldName = "IT", IndustryFieldCode = "A.01"},
            new DimIndustryFieldDbModel { IndustryFieldId = 2, IndustryFieldName = "Finance", IndustryFieldCode = "B.02"}
        );
        
        context.DimEmployees.Add(new DimEmployeeDbModel 
        { 
            EmployeeId = 1, 
            EmployeeRefId = "emp-1",
            BirthDate = new DateOnly(1990, 1, 1),
            CareerStartDate = new DateOnly(2015, 1, 1)
        });
        
        await context.SaveChangesAsync();

        context.DimJobs.AddRange(
            new DimJobDbModel { JobId = 1, StandardJobRoleTitle = "Engineer", HierarchyLevelName = "Mid", IndustryFieldId = 1 },
            new DimJobDbModel { JobId = 2, StandardJobRoleTitle = "Analyst", HierarchyLevelName = "Senior", IndustryFieldId = 2 }
        );
        
        context.DimEmployers.Add(new DimEmployerDbModel { EmployerId = 1, EmployerName = "Tech Corp", IndustryFieldId = 1 });
        
        await context.SaveChangesAsync();
    }
    
    [Fact]
    public async Task GetFactSalariesByFilterAsync_ShouldReturnFilteredData_WhenFiltersAreUsed()
    {
        _dbContext.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithId(1).WithDateId(1).WithLocationId(1).WithJobId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithId(2).WithDateId(2).WithLocationId(2).WithJobId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithId(3).WithDateId(3).WithLocationId(1).WithJobId(2).Build())
        );
        await _dbContext.SaveChangesAsync();
        var filters = new ResolvedSalaryFilter
        {
            LocationIds = new List<int> { 1 },
            JobIds = new List<int> { 1, 2 },
            DateStart = new DateOnly(2024, 1, 1),
            DateEnd = new DateOnly(2024, 6, 1)
        };
        var result = (await _sut.GetFactSalariesByFilterAsync(filters)).ToList();
        result.Should().HaveCount(2);
        result.Select(r => r.SalaryFactId).Should().Contain(new[] { 1L, 3L });
    }
}