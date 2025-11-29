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
        // Arrange: Insert specific facts linking to Static Dimensions (Ids 1-5 exist)
        _dbContext.FactSalaries.AddRange(
            // Matches Filter (DateId 1 = 2024-01-01, Loc 1, Job 1)
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithId(1).WithDateId(1).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            // Matches Filter (DateId 2 = 2024-02-01, Loc 2, Job 1)
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithId(2).WithDateId(2).WithLocationId(2).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            // Excluded by JobId (Job 2)
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithId(3).WithDateId(3).WithLocationId(1).WithJobId(2).WithEmployerId(1).WithEmployeeId(1).Build())
        );
        await _dbContext.SaveChangesAsync();

        var filters = new ResolvedSalaryFilter
        {
            LocationIds = new List<int> { 1, 2 }, // Both locations
            JobIds = new List<int> { 1 },         // Only Job 1
            DateStart = new DateOnly(2024, 1, 1),
            DateEnd = new DateOnly(2024, 6, 1)
        };

        // Act
        var result = (await _sut.GetFactSalariesByFilterAsync(filters)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(r => r.SalaryFactId).Should().Contain(new[] { 1L, 2L });
        result.Select(r => r.SalaryFactId).Should().NotContain(3L);
    }

    // 2. Authorized Method: Time Series (Complex Grouping Logic)
    [Fact]
    public async Task GetSalaryTimeSeriesAsync_ShouldGroupSalariesByQuarter_Correctly()
    {
        // Arrange
        // DateId 1 = 2024-01-01 (Q1)
        // DateId 2 = 2024-02-01 (Q1)
        // DateId 3 = 2024-05-01 (Q2)
        _dbContext.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithDateId(1).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithDateId(2).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(600).WithDateId(3).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build())
        );
        await _dbContext.SaveChangesAsync();

        var filters = new ResolvedSalaryFilter 
        { 
            // End date sets the reference point. Requesting 2 periods back from June -> Q1, Q2
            DateEnd = new DateOnly(2024, 6, 1) 
        };

        // Act: Request Quarterly granularity
        var result = await _sut.GetSalaryTimeSeriesAsync(filters, TimeGranularity.Quarter, 2);

        // Assert
        result.Should().HaveCount(2);

        // Q1 (Jan + Feb) -> (100 + 200) / 2 = 150
        var q1 = result.First(p => p.PeriodStart == new DateOnly(2024, 1, 1));
        q1.AvgSalary.Should().Be(150);
        q1.SalaryCountInPeriod.Should().Be(2);

        // Q2 (May) -> 600 / 1 = 600
        var q2 = result.First(p => p.PeriodStart == new DateOnly(2024, 4, 1)); // Q2 starts April 1st
        q2.AvgSalary.Should().Be(600);
        q2.SalaryCountInPeriod.Should().Be(1);
    }

    // 3. Authorized Method: Summary (Percentiles)
    [Fact]
    public async Task GetSalarySummaryAsync_ShouldCalculatePercentiles_Correctly()
    {
        // Arrange
        _dbContext.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithDateId(1).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithDateId(1).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(300).WithDateId(1).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build())
        );
        await _dbContext.SaveChangesAsync();

        var filters = new ResolvedSalaryFilter();

        // Act
        // Target 50th percentile (Median)
        var result = await _sut.GetSalarySummaryAsync(filters, 50);

        // Assert
        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(3);
        result.AverageSalary.Should().Be(200);
        result.Percentile50.Should().Be(200); // Median of 100, 200, 300 is 200
        result.PercentileTarget.Should().Be(200);
    }

    // 4. Public Method: Public Roles (Grouping by Job)
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