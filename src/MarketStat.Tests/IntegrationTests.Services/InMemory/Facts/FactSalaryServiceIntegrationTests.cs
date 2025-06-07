using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Exceptions;
using MarketStat.Services.Facts.FactSalaryService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Facts;

public class FactSalaryServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IFactSalaryService _factSalaryService;

    public FactSalaryServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _factSalaryService = _accessObject.FactSalaryService;
    }
    
    public void Dispose() => _accessObject.Dispose();

    [Fact]
    public async Task CreateFactSalaryAsync_ValidParameters_CreatesAndReturnsFact()
    {
        var fact = await _factSalaryService.CreateFactSalaryAsync(
            dateId: 1,
            cityId: 2,
            employerId: 3,
            jobRoleId: 4,
            employeeId: 5,
            salaryAmount: 1000m,
            bonusAmount: 100m
        );

        Assert.True(fact.SalaryFactId > 0);
        Assert.Equal(1, fact.DateId);
        Assert.Equal(2, fact.CityId);
        Assert.Equal(3, fact.EmployerId);
        Assert.Equal(4, fact.JobRoleId);
        Assert.Equal(5, fact.EmployeeId);
        Assert.Equal(1000m, fact.SalaryAmount);
        Assert.Equal(100m, fact.BonusAmount);

        var all = (await _factSalaryService.GetAllFactSalariesAsync()).ToList();
        Assert.Single(all);
        Assert.Equal(fact.SalaryFactId, all[0].SalaryFactId);
    }

    [Fact]
    public async Task CreateFactSalaryAsync_InvalidParameters_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _factSalaryService.CreateFactSalaryAsync(
                dateId: 1, cityId: 2, employerId: 3, jobRoleId: 4, employeeId: 5,
                salaryAmount: -100m, bonusAmount: 0m
            ));
    }
    
    [Fact]
    public async Task GetFactSalaryByIdAsync_Existing_ReturnsFact()
    {
        var seed = new FactSalary(42, 7, 8, 9, 10, 11, 2000m, 200m);
        await _accessObject.SeedSalaryAsync(new[] { seed });
    
        var fetched = await _factSalaryService.GetFactSalaryByIdAsync(42);
    
        Assert.Equal(42, fetched.SalaryFactId);
        Assert.Equal(7, fetched.DateId);
        Assert.Equal(8, fetched.CityId);
        Assert.Equal(9, fetched.EmployerId);
        Assert.Equal(10, fetched.JobRoleId);
        Assert.Equal(11, fetched.EmployeeId);
        Assert.Equal(2000m, fetched.SalaryAmount);
        Assert.Equal(200m, fetched.BonusAmount);
    }
    
    [Fact]
    public async Task GetFactSalaryByIdAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _factSalaryService.GetFactSalaryByIdAsync(999)
        );
    }
    
    [Fact]
    public async Task GetAllFactSalariesAsync_Seeded_ReturnsAll()
    {
        var seeds = new[]
        {
            new FactSalary(1, 1, 1, 1, 1, 1, 500m, 50m),
            new FactSalary(2, 1, 1, 1, 1, 2, 600m, 60m)
        };
        await _accessObject.SeedSalaryAsync(seeds);
    
        var list = (await _factSalaryService.GetAllFactSalariesAsync()).ToList();
    
        Assert.Equal(2, list.Count);
        Assert.Contains(list, f => f.SalaryFactId == 1);
        Assert.Contains(list, f => f.SalaryFactId == 2);
    }
    
    [Fact]
    public async Task UpdateFactSalaryAsync_Existing_UpdatesAndReturns()
    {
        var original = new FactSalary(5, 1, 1, 1, 1, 1, 300m, 30m);
        await _accessObject.SeedSalaryAsync(new[] { original });
    
        var updated = await _factSalaryService.UpdateFactSalaryAsync(
            salaryFactId: 5,
            dateId: 2,
            cityId: 2,
            employerId: 2,
            jobRoleId: 2,
            employeeId: 2,
            salaryAmount: 400m,
            bonusAmount: 40m
        );
    
        Assert.Equal(5, updated.SalaryFactId);
        Assert.Equal(400m, updated.SalaryAmount);
        Assert.Equal(40m, updated.BonusAmount);
    
        var fetched = await _factSalaryService.GetFactSalaryByIdAsync(5);
        Assert.Equal(400m, fetched.SalaryAmount);
    }
    
    [Fact]
    public async Task UpdateFactSalaryAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _factSalaryService.UpdateFactSalaryAsync(
                salaryFactId: 99,
                dateId: 1, cityId: 1, employerId: 1, jobRoleId: 1, employeeId: 1,
                salaryAmount: 100m, bonusAmount: 10m
            ));
    }
    
    [Fact]
    public async Task DeleteFactSalaryAsync_Existing_RemovesFact()
    {
        var seed = new FactSalary(7, 1, 1, 1, 1, 1, 50m, 10m);
        await _accessObject.SeedSalaryAsync(new[] { seed });
    
        await _factSalaryService.DeleteFactSalaryAsync(7);
    
        var all = (await _factSalaryService.GetAllFactSalariesAsync()).ToList();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task DeleteFactSalaryAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _factSalaryService.DeleteFactSalaryAsync(888)
        );
    }
}