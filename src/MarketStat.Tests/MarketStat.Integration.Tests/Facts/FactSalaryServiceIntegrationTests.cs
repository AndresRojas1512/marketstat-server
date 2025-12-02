using FluentAssertions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Requests;
using MarketStat.Common.Enums;
using MarketStat.Database.Context;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Database.Repositories.PostgresRepositories.Facts;
using MarketStat.Services.Facts.FactSalaryService;
using MarketStat.Tests.TestData.Builders.Facts;
using Microsoft.Extensions.Logging.Abstractions;

namespace MarketStat.Integration.Tests.Facts;

[Collection("Integration")]
public class FactSalaryServiceIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private readonly MarketStatDbContext _dbContext;
    private readonly FactSalaryService _sut;
    
    public FactSalaryServiceIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _dbContext = _fixture.CreateContext();

        var factRepo = new FactSalaryRepository(_dbContext);
        var locationRepo = new DimLocationRepository(_dbContext);
        var jobRepo = new DimJobRepository(_dbContext);
        var industryRepo = new DimIndustryFieldRepository(_dbContext);
        
        var logger = NullLogger<FactSalaryService>.Instance;

        _sut = new FactSalaryService(
            factRepo, 
            logger,
            locationRepo, 
            jobRepo, 
            industryRepo
        );
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
    public async Task GetFactSalariesByFilterAsync_ShouldResolveFiltersAndReturnList()
    {
        _dbContext.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithLocationId(1).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithLocationId(2).WithDateId(1).Build())
        );
        await _dbContext.SaveChangesAsync();

        var request = new Common.Core.MarketStat.Common.Core.Facts.Analytics.Requests.AnalysisFilterRequest
        {
            CityName = "Moscow"
        };

        var result = (await _sut.GetFactSalariesByFilterAsync(request)).ToList();

        result.Should().HaveCount(1);
        result.First().SalaryAmount.Should().Be(100);
    }
    
    [Fact]
    public async Task GetSalarySummaryAsync_ShouldResolveCityNameAndCalculateSummary()
    {
        _dbContext.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithLocationId(1).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithLocationId(1).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(500).WithLocationId(2).WithDateId(1).Build())
        );
        await _dbContext.SaveChangesAsync();

        var domainRequest = new Common.Core.MarketStat.Common.Core.Facts.Analytics.Requests.SalarySummaryRequest
        {
            CityName = "Moscow",
            TargetPercentile = 50
        };

        var result = await _sut.GetSalarySummaryAsync(domainRequest);

        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(2);
        result.AverageSalary.Should().Be(150);
    }

    [Fact]
    public async Task GetPublicRolesAsync_ShouldResolveIndustryAndFilterByRole()
    {
        _dbContext.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithJobId(1).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithJobId(1).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(500).WithJobId(2).WithDateId(1).Build())
        );
        await _dbContext.SaveChangesAsync();

        var request = new Common.Core.MarketStat.Common.Core.Facts.Analytics.Requests.PublicRolesRequest
        {
            IndustryFieldName = "IT",
            MinRecordCount = 1
        };
        
        var result = (await _sut.GetPublicRolesAsync(request)).ToList();
        
        result.Should().HaveCount(1);
        result.First().StandardJobRoleTitle.Should().Be("Engineer");
    }
    
    [Fact]
    public async Task GetSalaryTimeSeriesAsync_ShouldResolveFiltersAndReturnSeries()
    {
        _dbContext.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithDateId(1).WithLocationId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithDateId(2).WithLocationId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(500).WithDateId(1).WithLocationId(2).Build())
        );
        await _dbContext.SaveChangesAsync();

        var request = new Common.Core.MarketStat.Common.Core.Facts.Analytics.Requests.TimeSeriesRequest
        {
            CityName = "Moscow",
            Granularity = TimeGranularity.Month,
            Periods = 2,
            DateEnd = new DateOnly(2024, 2, 28)
        };

        var result = await _sut.GetSalaryTimeSeriesAsync(request);

        result.Should().HaveCount(2);
        
        var jan = result.First(p => p.PeriodStart == new DateOnly(2024, 1, 1));
        jan.AvgSalary.Should().Be(100); 
        
        var feb = result.First(p => p.PeriodStart == new DateOnly(2024, 2, 1));
        feb.AvgSalary.Should().Be(200);
    }

    [Fact]
    public async Task GetSalaryTimeSeriesAsync_ShouldReturnEmpty_WhenFilterResolvesToNothing()
    {
        _dbContext.FactSalaries.Add(FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithLocationId(1).Build()));
        await _dbContext.SaveChangesAsync();

        var request = new Common.Core.MarketStat.Common.Core.Facts.Analytics.Requests.TimeSeriesRequest
        {
            CityName = "NonExistentCity",
            Granularity = TimeGranularity.Month,
            Periods = 1
        };
        var result = await _sut.GetSalaryTimeSeriesAsync(request);
        result.Should().BeEmpty();
    }

}