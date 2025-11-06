using FluentAssertions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Models;
using MarketStat.Database.Repositories.PostgresRepositories.Facts;
using MarketStat.Tests.TestData.Builders.Facts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace MarketStat.Repository.Tests.Facts;

public class FactSalaryRepositoryTests
{
    private MarketStatDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MarketStatDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        var context = new MarketStatDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
    
    private FactSalaryRepository CreateRepository(MarketStatDbContext context)
    {
        var mockLogger = new Mock<ILogger<FactSalaryRepository>>();
        return new FactSalaryRepository(context, mockLogger.Object);
    }
    
    private async Task SeedDimensionsAsync(MarketStatDbContext context)
    {
        context.DimDates.Add(new DimDateDbModel { DateId = 1, FullDate = new DateOnly(2024, 1, 1), Year = 2024, Quarter = 1, Month = 1 });
        context.DimDates.Add(new DimDateDbModel { DateId = 2, FullDate = new DateOnly(2024, 2, 1), Year = 2024, Quarter = 1, Month = 2 });
        context.DimDates.Add(new DimDateDbModel { DateId = 3, FullDate = new DateOnly(2024, 5, 1), Year = 2024, Quarter = 2, Month = 5 });
        context.DimDates.Add(new DimDateDbModel { DateId = 4, FullDate = new DateOnly(2024, 8, 1), Year = 2024, Quarter = 3, Month = 8 });
        
        context.DimLocations.Add(new DimLocationDbModel { LocationId = 1, CityName = "Moscow", OblastName = "Moscow", DistrictName = "Central" });
        context.DimLocations.Add(new DimLocationDbModel { LocationId = 2, CityName = "Tula", OblastName = "Tula", DistrictName = "Central" });
        
        context.DimJobs.Add(new DimJobDbModel { JobId = 1, StandardJobRoleTitle = "Engineer", HierarchyLevelName = "Mid", IndustryFieldId = 1 });
        context.DimJobs.Add(new DimJobDbModel { JobId = 2, StandardJobRoleTitle = "Analyst", HierarchyLevelName = "Senior", IndustryFieldId = 2 });
        
        context.DimEmployers.Add(new DimEmployerDbModel { EmployerId = 1, EmployerName = "Tech Corp" });
        context.DimEmployees.Add(new DimEmployeeDbModel { EmployeeId = 1, EmployeeRefId = "emp-1" });
        
        await context.SaveChangesAsync();
    }
    
    [Fact]
    public async Task AddFactSalaryAsync_ShouldAddSalary_WhenDataIsCorrect()
    {
        await using var context = CreateInMemoryDbContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        var newSalary = new FactSalaryBuilder()
            .WithId(0)
            .WithSalaryAmount(120000)
            .Build();
        await repository.AddFactSalaryAsync(newSalary);
        var savedSalary = await context.FactSalaries.FindAsync(newSalary.SalaryFactId);
        savedSalary.Should().NotBeNull();
        savedSalary.SalaryAmount.Should().Be(120000);
        newSalary.SalaryFactId.Should().Be(1L);
    }

    [Fact]
    public async Task AddFactSalaryAsync_ShouldThrowNotFoundException_WhenForeignKeyIsMissing()
    {
        await using var context = CreateInMemoryDbContext();
        var repository = CreateRepository(context);
        var newSalary = new FactSalaryBuilder().WithDateId(999).Build();
        Func<Task> act = async () => await repository.AddFactSalaryAsync(newSalary); 
        Func<Task> nullAct = async () => await repository.AddFactSalaryAsync(null!);
        await nullAct.Should().ThrowAsync<Exception>();
    }
    
    [Fact]
    public async Task GetFactSalaryByIdAsync_ShouldReturnSalary_WhenSalaryExists()
    {
        await using var context = CreateInMemoryDbContext();
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
        await using var context = CreateInMemoryDbContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.GetFactSalaryByIdAsync(999L);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task UpdateFactSalaryAsync_ShouldUpdateSalary_WhenSalaryExists()
    {
        await using var context = CreateInMemoryDbContext();
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
        await using var context = CreateInMemoryDbContext();
        var repository = CreateRepository(context);
        var nonExistentSalary = new FactSalaryBuilder().WithId(999L).Build();
        Func<Task> act = async () => await repository.UpdateFactSalaryAsync(nonExistentSalary);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task DeleteFactSalaryByIdAsync_ShouldDeleteSalary_WhenSalaryExists()
    {
        await using var context = CreateInMemoryDbContext();
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
        await using var context = CreateInMemoryDbContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.DeleteFactSalaryByIdAsync(999L);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetFactSalariesByFilterAsync_ShouldReturnFilteredData_WhenFiltersAreUsed()
    {
        await using var context = CreateInMemoryDbContext();
        await SeedDimensionsAsync(context);
        var repository = CreateRepository(context);
        context.FactSalaries.AddRange(
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithId(1).WithDateId(1).WithLocationId(1).WithJobId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithId(2).WithDateId(2).WithLocationId(2).WithJobId(1).Build()),
            FactSalaryConverter.ToDbModel(new FactSalaryBuilder().WithId(3).WithDateId(3).WithLocationId(1).WithJobId(2).Build())
        );
        await context.SaveChangesAsync();
        var filters = new ResolvedSalaryFilterDto
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
}