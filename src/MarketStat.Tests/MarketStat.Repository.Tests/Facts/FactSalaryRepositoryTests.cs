using FluentAssertions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Enums;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Models;
using MarketStat.Database.Repositories.PostgresRepositories.Facts;
using MarketStat.Tests.TestData.Builders.Facts;
using Microsoft.Extensions.Logging;
using Moq;

namespace MarketStat.Repository.Tests.Facts;

[Collection("Database collection")]
public class FactSalaryRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public FactSalaryRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    private FactSalaryRepository CreateRepository(MarketStatDbContext context)
    {
        return new FactSalaryRepository(context);
    }
    
    private async Task SeedDimensionsAsync(MarketStatDbContext context)
    {
        context.DimDates.Add(new DimDateDbModel { DateId = 1, FullDate = new DateOnly(2024, 1, 1), Year = 2024, Quarter = 1, Month = 1 });
        context.DimDates.Add(new DimDateDbModel { DateId = 2, FullDate = new DateOnly(2024, 2, 1), Year = 2024, Quarter = 1, Month = 2 });
        context.DimDates.Add(new DimDateDbModel { DateId = 3, FullDate = new DateOnly(2024, 5, 1), Year = 2024, Quarter = 2, Month = 5 });
        context.DimDates.Add(new DimDateDbModel { DateId = 4, FullDate = new DateOnly(2024, 8, 1), Year = 2024, Quarter = 3, Month = 8 });
        context.DimDates.Add(new DimDateDbModel { DateId = 5, FullDate = new DateOnly(2019, 1, 1), Year = 2019, Quarter = 1, Month = 1 });
        
        context.DimLocations.Add(new DimLocationDbModel { LocationId = 1, CityName = "Moscow", OblastName = "Moscow", DistrictName = "Central" });
        context.DimLocations.Add(new DimLocationDbModel { LocationId = 2, CityName = "Tula", OblastName = "Tula", DistrictName = "Central" });
        
        context.DimIndustryFields.Add(new DimIndustryFieldDbModel { IndustryFieldId = 1, IndustryFieldName = "IT", IndustryFieldCode = "A.01"});
        context.DimIndustryFields.Add(new DimIndustryFieldDbModel { IndustryFieldId = 2, IndustryFieldName = "Finance", IndustryFieldCode = "B.02"});
        
        context.DimJobs.Add(new DimJobDbModel { JobId = 1, StandardJobRoleTitle = "Engineer", HierarchyLevelName = "Mid", IndustryFieldId = 1 });
        context.DimJobs.Add(new DimJobDbModel { JobId = 2, StandardJobRoleTitle = "Analyst", HierarchyLevelName = "Senior", IndustryFieldId = 2 });
        
        context.DimEmployers.Add(new DimEmployerDbModel { EmployerId = 1, EmployerName = "Tech Corp" });
        context.DimEmployees.Add(new DimEmployeeDbModel { EmployeeId = 1, EmployeeRefId = "emp-1" });
        
        await context.SaveChangesAsync();
    }
    
    [Fact]
    public async Task AddFactSalaryAsync_ShouldAddSalary_WhenDataIsCorrect()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        var newSalary = new FactSalaryBuilder()
            .WithId(0)
            .WithSalaryAmount(120000)
            .Build();
        await repository.AddFactSalaryAsync(newSalary);
        newSalary.SalaryFactId.Should().BeGreaterThan(0);
        var savedSalary = await context.FactSalaries.FindAsync(newSalary.SalaryFactId);
        savedSalary.Should().NotBeNull();
        savedSalary!.SalaryAmount.Should().Be(120000);
    }
    
    [Fact]
    public async Task AddFactSalaryAsync_ShouldThrowException_WhenSalaryIsNull()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.AddFactSalaryAsync(null!);
        await act.Should().ThrowAsync<Exception>();
    }
    
    [Fact]
    public async Task GetFactSalaryByIdAsync_ShouldReturnSalary_WhenSalaryExists()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        var existingSalary = new FactSalaryBuilder().WithId(1L).Build();
        context.FactSalaries.Add(FactSalaryConverter.ToDbModel(existingSalary));
        await context.SaveChangesAsync();
        var result = await repository.GetFactSalaryByIdAsync(1L);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(existingSalary);
    }

    [Fact]
    public async Task GetFactSalaryByIdAsync_ShouldThrowNotFoundException_WhenSalaryDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.GetFactSalaryByIdAsync(999L);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task UpdateFactSalaryAsync_ShouldUpdateSalary_WhenSalaryExists()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        var originalModel = FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithId(1L).WithSalaryAmount(100).Build());
        context.FactSalaries.Add(originalModel);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var updatedSalary = new FactSalaryBuilder().WithId(1L).WithSalaryAmount(200).Build();
        await repository.UpdateFactSalaryAsync(updatedSalary);
        var salaryInDb = await context.FactSalaries.FindAsync(1L);
        salaryInDb.Should().NotBeNull();
        salaryInDb.SalaryAmount.Should().Be(200);
    }

    [Fact]
    public async Task UpdateFactSalaryAsync_ShouldThrowNotFoundException_WhenSalaryDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var nonExistentSalary = new FactSalaryBuilder().WithId(999L).Build();
        Func<Task> act = async () => await repository.UpdateFactSalaryAsync(nonExistentSalary);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task DeleteFactSalaryByIdAsync_ShouldDeleteSalary_WhenSalaryExists()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        var dbModel = FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithId(1L).Build());
        context.FactSalaries.Add(dbModel);
        await context.SaveChangesAsync();
        (await context.FactSalaries.FindAsync(1L)).Should().NotBeNull();
        await repository.DeleteFactSalaryByIdAsync(1L);
        (await context.FactSalaries.FindAsync(1L)).Should().BeNull();
    }
    
    [Fact]
    public async Task DeleteFactSalaryByIdAsync_ShouldThrowNotFoundException_WhenSalaryDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.DeleteFactSalaryByIdAsync(999L);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetFactSalariesByFilterAsync_ShouldReturnFilteredData_WhenFiltersAreUsed()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        context.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithId(1).WithDateId(1).WithLocationId(1).WithJobId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithId(2).WithDateId(2).WithLocationId(2).WithJobId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithId(3).WithDateId(3).WithLocationId(1).WithJobId(2).Build())
        );
        await context.SaveChangesAsync();
        var filters = new ResolvedSalaryFilter
        {
            LocationIds = new List<int> { 1 },
            JobIds = new List<int> { 1, 2 },
            DateStart = new DateOnly(2024, 1, 1),
            DateEnd = new DateOnly(2024, 6, 1)
        };
        var result = (await repository.GetFactSalariesByFilterAsync(filters)).ToList();
        result.Should().HaveCount(2);
        result.Select(r => r.SalaryFactId).Should().Contain(new[] { 1L, 3L });
    }
    
    [Fact]
    public async Task GetSalarySummaryAsync_ShouldCalculateCorrectSummary()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        context.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(300).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(400).WithDateId(1).Build())
        );
        await context.SaveChangesAsync();
        var filters = new ResolvedSalaryFilter();
        var result = await repository.GetSalarySummaryAsync(filters, 90);
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(4);
        result.AverageSalary.Should().Be(250);
        result.Percentile50.Should().Be(250);
        result.PercentileTarget.Should().Be(370);
    }
    
    [Fact]
    public async Task GetSalarySummaryAsync_ShouldReturnNull_WhenNoSalariesMatchFilter()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        var filters = new ResolvedSalaryFilter { DateStart = new DateOnly(2099, 1, 1) };
        var result = await repository.GetSalarySummaryAsync(filters, 90);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSalarySummaryAsync_ShouldCalculateCorrectly_WhenSalaryCountIsOdd()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        context.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(600).WithDateId(1).Build())
        );
        await context.SaveChangesAsync();
        var filters = new ResolvedSalaryFilter();
        var result = await repository.GetSalarySummaryAsync(filters, 50);
        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(3);
        result.AverageSalary.Should().Be(300);
        result.Percentile50.Should().Be(200); 
        result.PercentileTarget.Should().Be(200);
    }

    [Fact]
    public async Task GetSalarySummaryAsync_ShouldHandlePercentile0And100_Correctly()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);

        context.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(300).WithDateId(1).Build())
        );
        await context.SaveChangesAsync();
        var filters = new ResolvedSalaryFilter();
        var resultP0 = await repository.GetSalarySummaryAsync(filters, 0);
        var resultP100 = await repository.GetSalarySummaryAsync(filters, 100);
        resultP0.Should().NotBeNull();
        resultP0!.PercentileTarget.Should().Be(100);
        resultP100.Should().NotBeNull();
        resultP100!.PercentileTarget.Should().Be(300);
    }

    [Fact]
    public async Task GetSalaryDistributionAsync_ShouldCreateBuckets()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        context.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(110).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(300).WithDateId(1).Build())
        );
        await context.SaveChangesAsync();
        var filters = new ResolvedSalaryFilter();
        var result = await repository.GetSalaryDistributionAsync(filters);
        result.Should().NotBeNull();
        result.Sum(b => b.BucketCount).Should().Be(4);
        result.First().LowerBound.Should().Be(100);
        result.First().BucketCount.Should().Be(2);
    }
    
    [Fact]
    public async Task GetSalaryDistributionAsync_ShouldReturnEmptyList_WhenNoSalariesMatchFilter()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        var filters = new ResolvedSalaryFilter { DateStart = new DateOnly(2099, 1, 1) };
        var result = await repository.GetSalaryDistributionAsync(filters);
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSalaryDistributionAsync_ShouldReturnSingleBucket_WhenOnlyOneSalaryExists()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        context.FactSalaries.Add(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(150).WithDateId(1).Build())
        );
        await context.SaveChangesAsync();
        var filters = new ResolvedSalaryFilter();
        var result = await repository.GetSalaryDistributionAsync(filters);
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].BucketCount.Should().Be(1);
        result[0].LowerBound.Should().Be(150);
        result[0].UpperBound.Should().Be(150);
    }

    [Fact]
    public async Task GetSalaryDistributionAsync_ShouldReturnSingleBucket_WhenAllSalariesAreIdentical()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);

        context.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithDateId(2).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithDateId(3).Build())
        );
        await context.SaveChangesAsync();
        var filters = new ResolvedSalaryFilter();
        var result = await repository.GetSalaryDistributionAsync(filters);
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].BucketCount.Should().Be(3);
        result[0].LowerBound.Should().Be(100);
        result[0].UpperBound.Should().Be(100);
    }
    
    [Fact]
    public async Task GetSalaryTimeSeriesAsync_ShouldCreateTimeSeriesByMonth()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        context.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(300).WithDateId(2).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(500).WithDateId(3).Build())
        );
        await context.SaveChangesAsync();
        var filters = new ResolvedSalaryFilter { DateEnd = new DateOnly(2024, 5, 31) };
        var result = await repository.GetSalaryTimeSeriesAsync(filters, TimeGranularity.Month, 6);
        result.Should().HaveCount(6);
        
        var jan = result.First(p => p.PeriodStart == new DateOnly(2024, 1, 1));
        jan.AvgSalary.Should().Be(150);
        jan.SalaryCountInPeriod.Should().Be(2);

        var feb = result.First(p => p.PeriodStart == new DateOnly(2024, 2, 1));
        feb.AvgSalary.Should().Be(300);
        feb.SalaryCountInPeriod.Should().Be(1);
        
        var mar = result.First(p => p.PeriodStart == new DateOnly(2024, 3, 1));
        mar.AvgSalary.Should().Be(0);
        mar.SalaryCountInPeriod.Should().Be(0);

        var may = result.First(p => p.PeriodStart == new DateOnly(2024, 5, 1));
        may.AvgSalary.Should().Be(500);
        may.SalaryCountInPeriod.Should().Be(1);
    }
    
    [Fact]
    public async Task GetSalaryTimeSeriesAsync_ShouldReturnAllZeroes_WhenNoSalariesMatchFilter()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        context.FactSalaries.Add(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithDateId(1).Build())
        );
        await context.SaveChangesAsync();
        var filters = new ResolvedSalaryFilter { DateStart = new DateOnly(2099, 1, 1) };
        var result = await repository.GetSalaryTimeSeriesAsync(filters, TimeGranularity.Month, 4);
        result.Should().HaveCount(4);
        result.Should().OnlyContain(p => p.AvgSalary == 0 && p.SalaryCountInPeriod == 0);
    }

    [Fact]
    public async Task GetSalaryTimeSeriesAsync_ShouldCorrectlyGroupBy_Quarter()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        
        context.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithDateId(1).Build()), // Q1
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithDateId(2).Build()), // Q1
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(600).WithDateId(3).Build())  // Q2
        );
        await context.SaveChangesAsync();
        
        var filters = new ResolvedSalaryFilter { DateEnd = new DateOnly(2024, 6, 1) };

        var result = await repository.GetSalaryTimeSeriesAsync(filters, TimeGranularity.Quarter, 2);

        result.Should().HaveCount(2);

        var q1 = result.First(p => p.PeriodStart == new DateOnly(2024, 1, 1));
        q1.AvgSalary.Should().Be(150);
        q1.SalaryCountInPeriod.Should().Be(2);

        var q2 = result.First(p => p.PeriodStart == new DateOnly(2024, 4, 1));
        q2.AvgSalary.Should().Be(600);
        q2.SalaryCountInPeriod.Should().Be(1);
    }

    [Fact]
    public async Task GetSalaryTimeSeriesAsync_ShouldCorrectlyGroupBy_Year()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        
        context.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithDateId(1).Build()), // 2024
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithDateId(2).Build()), // 2024
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(600).WithDateId(5).Build())  // 2019
        );
        await context.SaveChangesAsync();
        
        var filters = new ResolvedSalaryFilter { DateEnd = new DateOnly(2024, 12, 31) };

        var result = await repository.GetSalaryTimeSeriesAsync(filters, TimeGranularity.Year, 6);

        result.Should().HaveCount(6);

        var year2019 = result.First(p => p.PeriodStart == new DateOnly(2019, 1, 1));
        year2019.AvgSalary.Should().Be(600);
        year2019.SalaryCountInPeriod.Should().Be(1);

        var year2024 = result.First(p => p.PeriodStart == new DateOnly(2024, 1, 1));
        year2024.AvgSalary.Should().Be(150);
        year2024.SalaryCountInPeriod.Should().Be(2);

        var year2023 = result.First(p => p.PeriodStart == new DateOnly(2023, 1, 1));
        year2023.AvgSalary.Should().Be(0);
        year2023.SalaryCountInPeriod.Should().Be(0);
    }

    [Fact]
    public async Task GetPublicRolesAsync_ShouldReturnAggregatedRoles()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        
        context.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithJobId(1).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithJobId(1).WithDateId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(300).WithJobId(2).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(10).WithJobId(1).WithDateId(5).Build())
        );
        await context.SaveChangesAsync();
        var filters = new ResolvedSalaryFilter { DateStart = new DateOnly(2020, 1, 1) };
        var result = (await repository.GetPublicRolesAsync(filters, 2)).ToList();
        result.Should().HaveCount(1); 
        result[0].StandardJobRoleTitle.Should().Be("Engineer");
        result[0].AverageSalary.Should().Be(150);
        result[0].SalaryRecordCount.Should().Be(2);
    }

    [Fact]
    public async Task GetPublicRolesAsync_ShouldReturnEmpty_WhenMinCountIsNotMet()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        
        context.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithJobId(1).Build())
        );
        await context.SaveChangesAsync();
        var filters = new ResolvedSalaryFilter();
        var result = (await repository.GetPublicRolesAsync(filters, 5)).ToList();
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetPublicRolesAsync_ShouldReturnAllGroups_WhenMinCountIsOne()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        
        context.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(100).WithJobId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(200).WithJobId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(500).WithJobId(2).Build())
        );
        await context.SaveChangesAsync();
        var filters = new ResolvedSalaryFilter();
        var result = (await repository.GetPublicRolesAsync(filters, 1)).ToList();
        result.Should().HaveCount(2); 
    }

    [Fact]
    public async Task GetPublicRolesAsync_ShouldCorrectlyOrderBy_AverageSalaryDescending()
    {
        await using var context = _fixture.CreateCleanContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        
        context.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(400).WithJobId(2).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(600).WithJobId(2).Build()),
            
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(900).WithJobId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithSalaryAmount(900).WithJobId(1).Build())
        );
        await context.SaveChangesAsync();
        var filters = new ResolvedSalaryFilter();
        var result = (await repository.GetPublicRolesAsync(filters, 2)).ToList();
        result.Should().HaveCount(2);
        result[0].StandardJobRoleTitle.Should().Be("Engineer");
        result[0].AverageSalary.Should().Be(900);
        result[1].StandardJobRoleTitle.Should().Be("Analyst");
        result[1].AverageSalary.Should().Be(500);
    }
}