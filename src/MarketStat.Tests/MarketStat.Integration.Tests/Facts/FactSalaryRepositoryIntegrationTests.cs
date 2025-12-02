using FluentAssertions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Enums;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Models;
using MarketStat.Database.Repositories.PostgresRepositories.Facts;
using MarketStat.Tests.TestData.Builders.Facts;
using Microsoft.EntityFrameworkCore;
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
    public async Task AddFactSalaryAsync_ShouldAddSalary_WhenDataIsCorrect()
    {
        var newSalary = new FactSalaryBuilder()
            .WithId(0)
            .WithSalaryAmount(120000)
            .WithDateId(1).WithLocationId(1).WithEmployerId(1).WithJobId(1).WithEmployeeId(1)
            .Build();

        await _sut.AddFactSalaryAsync(newSalary);
        
        newSalary.SalaryFactId.Should().BeGreaterThan(0);
        var savedSalary = await _dbContext.FactSalaries.AsNoTracking().FirstOrDefaultAsync(f => f.SalaryFactId == newSalary.SalaryFactId);
        savedSalary.Should().NotBeNull();
        savedSalary!.SalaryAmount.Should().Be(120000);
    }
    
    [Fact]
    public async Task GetFactSalaryByIdAsync_ShouldReturnSalary_WhenSalaryExists()
    {
        var existingSalary = new FactSalaryBuilder()
            .WithId(0)
            .WithDateId(1).WithLocationId(1).WithEmployerId(1).WithJobId(1).WithEmployeeId(1)
            .Build();
        
        var dbModel = FactSalaryConverter.ToDbModel(existingSalary);
        _dbContext.FactSalaries.Add(dbModel);
        await _dbContext.SaveChangesAsync();
        
        var result = await _sut.GetFactSalaryByIdAsync(dbModel.SalaryFactId);

        result.Should().NotBeNull();
        result.SalaryFactId.Should().Be(dbModel.SalaryFactId);
    }
    
    [Fact]
    public async Task GetFactSalaryByIdAsync_ShouldThrowNotFoundException_WhenSalaryDoesNotExist()
    {
        Func<Task> act = async () => await _sut.GetFactSalaryByIdAsync(99999);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task UpdateFactSalaryAsync_ShouldUpdateSalary_WhenSalaryExists()
    {
        var builder = new FactSalaryBuilder()
            .WithId(0).WithSalaryAmount(100)
            .WithDateId(1).WithLocationId(1).WithEmployerId(1).WithJobId(1).WithEmployeeId(1);
            
        var dbModel = FactSalaryConverter.ToDbModel(builder.Build());
        _dbContext.FactSalaries.Add(dbModel);
        await _dbContext.SaveChangesAsync();
        
        _dbContext.ChangeTracker.Clear();

        var salaryToUpdate = builder.WithId(dbModel.SalaryFactId).WithSalaryAmount(200).Build();
        await _sut.UpdateFactSalaryAsync(salaryToUpdate);

        var updatedDb = await _dbContext.FactSalaries.AsNoTracking().FirstOrDefaultAsync(f => f.SalaryFactId == dbModel.SalaryFactId);
        updatedDb.Should().NotBeNull();
        updatedDb!.SalaryAmount.Should().Be(200);
    }
    
    [Fact]
    public async Task UpdateFactSalaryAsync_ShouldThrowNotFoundException_WhenSalaryDoesNotExist()
    {
        var nonExistent = new FactSalaryBuilder().WithId(99999).Build();
        Func<Task> act = async () => await _sut.UpdateFactSalaryAsync(nonExistent);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task DeleteFactSalaryByIdAsync_ShouldDeleteSalary_WhenSalaryExists()
    {
        var dbModel = FactSalaryConverter.ToDbModel(new FactSalaryBuilder()
            .WithId(0)
            .WithDateId(1).WithLocationId(1).WithEmployerId(1).WithJobId(1).WithEmployeeId(1)
            .Build());
        _dbContext.FactSalaries.Add(dbModel);
        await _dbContext.SaveChangesAsync();

        await _sut.DeleteFactSalaryByIdAsync(dbModel.SalaryFactId);

        var deleted = await _dbContext.FactSalaries.AsNoTracking().FirstOrDefaultAsync(f => f.SalaryFactId == dbModel.SalaryFactId);
        deleted.Should().BeNull();
    }
    
    [Fact]
    public async Task DeleteFactSalaryByIdAsync_ShouldThrowNotFoundException_WhenSalaryDoesNotExist()
    {
        Func<Task> act = async () => await _sut.DeleteFactSalaryByIdAsync(99999);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetSalaryDistributionAsync_ShouldCreateBuckets()
    {
        _dbContext.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithDateId(1).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(110).WithDateId(1).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithDateId(1).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(300).WithDateId(1).WithLocationId(1).WithJobId(1).WithEmployerId(1).WithEmployeeId(1).Build())
        );
        await _dbContext.SaveChangesAsync();

        var filters = new ResolvedSalaryFilter();
        
        var result = await _sut.GetSalaryDistributionAsync(filters);

        result.Should().NotBeNull();
        result.Sum(b => b.BucketCount).Should().Be(4);
        result.First().LowerBound.Should().Be(100);
        result.First().BucketCount.Should().Be(2);
    }
    
    [Fact]
    public async Task GetSalaryDistributionAsync_ShouldReturnEmptyList_WhenNoSalariesMatchFilter()
    {
        var filters = new ResolvedSalaryFilter { DateStart = new DateOnly(2099, 1, 1) };
        var result = await _sut.GetSalaryDistributionAsync(filters);
        result.Should().BeEmpty();
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
        _dbContext.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithJobId(1).WithDateId(1).WithLocationId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithJobId(1).WithDateId(1).WithLocationId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(300).WithJobId(1).WithDateId(1).WithLocationId(1).WithEmployerId(1).WithEmployeeId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(500).WithJobId(2).WithDateId(1).WithLocationId(1).WithEmployerId(1).WithEmployeeId(1).Build())
        );
        await _dbContext.SaveChangesAsync();

        var filters = new ResolvedSalaryFilter();

        var result = (await _sut.GetPublicRolesAsync(filters, 2)).ToList();

        result.Should().HaveCount(1);
        var engineerRole = result.First();
        
        engineerRole.StandardJobRoleTitle.Should().Be("Engineer");
        engineerRole.SalaryRecordCount.Should().Be(3);
        engineerRole.AverageSalary.Should().Be(200);
    }
}