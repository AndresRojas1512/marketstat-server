using FluentAssertions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Enums;
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
        return _fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
    
    [Fact]
    public async Task GetFactSalariesByFilterAsync_ShouldReturnFilteredData_WhenFiltersAreUsed()
    {
        _dbContext.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithId(1).WithDateId(1).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithId(2).WithDateId(2).WithLocationId(2).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithId(3).WithDateId(3).WithLocationId(1).WithJobId(2).WithEmployerId(1).WithEmployeeId(1).Build())
        );
        await _dbContext.SaveChangesAsync();

        var filters = new ResolvedSalaryFilter
        {
            LocationIds = new List<int> { 1, 2 },
            JobIds = new List<int> { 1 },
            DateStart = new DateOnly(2024, 1, 1),
            DateEnd = new DateOnly(2024, 6, 1)
        };

        var result = (await _sut.GetFactSalariesByFilterAsync(filters)).ToList();
        
        result.Should().HaveCount(2);
        result.Select(r => r.SalaryFactId).Should().Contain(new[] { 1L, 2L });
        result.Select(r => r.SalaryFactId).Should().NotContain(3L);
    }

    [Fact]
    public async Task GetSalaryTimeSeriesAsync_ShouldGroupSalariesByQuarter_Correctly()
    {
        _dbContext.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithDateId(1).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithDateId(2).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(600).WithDateId(3).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build())
        );
        await _dbContext.SaveChangesAsync();

        var filters = new ResolvedSalaryFilter 
        { 
            DateEnd = new DateOnly(2024, 6, 1) 
        };

        var result = await _sut.GetSalaryTimeSeriesAsync(filters, TimeGranularity.Quarter, 2);

        result.Should().HaveCount(2);

        var q1 = result.First(p => p.PeriodStart == new DateOnly(2024, 1, 1));
        q1.AvgSalary.Should().Be(150);
        q1.SalaryCountInPeriod.Should().Be(2);

        var q2 = result.First(p => p.PeriodStart == new DateOnly(2024, 4, 1));
        q2.AvgSalary.Should().Be(600);
        q2.SalaryCountInPeriod.Should().Be(1);
    }

    [Fact]
    public async Task GetSalarySummaryAsync_ShouldCalculatePercentiles_Correctly()
    {
        _dbContext.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithDateId(1).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithDateId(1).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(300).WithDateId(1).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build())
        );
        await _dbContext.SaveChangesAsync();

        var filters = new ResolvedSalaryFilter();

        var result = await _sut.GetSalarySummaryAsync(filters, 50);

        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(3);
        result.AverageSalary.Should().Be(200);
        result.Percentile50.Should().Be(200);
        result.PercentileTarget.Should().Be(200);
    }

    [Fact]
    public async Task GetPublicRolesAsync_ShouldAggregateAndFilterByMinCount()
    {
        // Arrange
        // Job 1 (Engineer): 3 entries (Avg: 200)
        // Job 2 (Analyst): 1 entry (Avg: 500)
        _dbContext.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithJobId(1).WithDateId(1).WithLocationId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithJobId(1).WithDateId(1).WithLocationId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(300).WithJobId(1).WithDateId(1).WithLocationId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(500).WithJobId(2).WithDateId(1).WithLocationId(1).WithEmployerId(1).WithEmployeeId(1).Build())
        );
        await _dbContext.SaveChangesAsync();

        var filters = new ResolvedSalaryFilter();

        // Act: Request roles with at least 2 records
        var result = (await _sut.GetPublicRolesAsync(filters, 2)).ToList();

        // Assert
        result.Should().HaveCount(1); // Only Engineer should appear
        var engineerRole = result.First();
        
        // Note: StandardJobRoleTitle for JobId 1 was seeded as "Engineer" in Fixture
        engineerRole.StandardJobRoleTitle.Should().Be("Engineer");
        engineerRole.SalaryRecordCount.Should().Be(3);
        engineerRole.AverageSalary.Should().Be(200);
    }
}