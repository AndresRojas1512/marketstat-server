using MarketStat.Common.Core.Facts.Analytics.Requests;

namespace MarketStat.Integration.Tests.Facts;

using FluentAssertions;
using MarketStat.Common.Converter.Facts;
using MarketStat.Common.Dto.Facts.Analytics.Requests;
using MarketStat.Common.Enums;
using MarketStat.Database.Context;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Database.Repositories.PostgresRepositories.Facts;
using MarketStat.Services.Facts.FactSalaryService;
using MarketStat.Tests.TestData.Builders.Facts;
using Microsoft.Extensions.Logging.Abstractions;

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
            industryRepo);
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
    public async Task GetSalarySummaryAsync_ShouldResolveCityNameAndCalculateSummary()
    {
        _dbContext.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithLocationId(1).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithLocationId(1).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(500).WithLocationId(2).WithDateId(1).Build()));
        await _dbContext.SaveChangesAsync();

        var request = new SalarySummaryRequestDto
        {
            CityName = "Moscow",
            TargetPercentile = 50,
        };

        var domainRequest = new SalarySummaryRequest
        {
            CityName = "Moscow",
            TargetPercentile = 50,
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
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(500).WithJobId(2).WithDateId(1).Build()));
        await _dbContext.SaveChangesAsync();

        var request = new PublicRolesRequest
        {
            IndustryFieldName = "IT",
            MinRecordCount = 1,
        };
        var result = (await _sut.GetPublicRolesAsync(request)).ToList();
        result.Should().HaveCount(1);
        result.First().StandardJobRoleTitle.Should().Be("Engineer");
        result.First().SalaryRecordCount.Should().Be(2);
    }

    [Fact]
    public async Task GetSalaryTimeSeriesAsync_ShouldReturnEmpty_WhenFilterResolvesToNothing()
    {
        _dbContext.FactSalaries.Add(FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithLocationId(1).Build()));
        await _dbContext.SaveChangesAsync();

        var request = new TimeSeriesRequest
        {
            CityName = "NonExistentCity",
            Granularity = TimeGranularity.Month,
            Periods = 1,
        };
        var result = await _sut.GetSalaryTimeSeriesAsync(request);
        result.Should().BeEmpty();
    }
}
