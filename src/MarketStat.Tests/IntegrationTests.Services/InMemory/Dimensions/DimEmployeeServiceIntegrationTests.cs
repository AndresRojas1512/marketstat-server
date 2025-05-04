using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Services.Dimensions.DimEmployeeService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimEmployeeServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimEmployeeService _dimEmployeeService;

    public DimEmployeeServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimEmployeeService =
            new DimEmployeeService(_accessObject.DimEmployeeRepository, NullLogger<DimEmployeeService>.Instance);
    }
    
    public void Dispose() => _accessObject.Dispose();
    
    [Fact]
    public async Task CreateEmployeeAsync_ValidDates_CreatesAndReturnsNewEmployee()
    {
        var birth = DateOnly.Parse("1990-01-15");
        var start = DateOnly.Parse("2010-06-01");

        var emp = await _dimEmployeeService.CreateEmployeeAsync(birth, start);

        Assert.True(emp.EmployeeId > 0);
        Assert.Equal(birth, emp.BirthDate);
        Assert.Equal(start, emp.CareerStartDate);

        var all = (await _dimEmployeeService.GetAllEmployeesAsync()).ToList();
        Assert.Single(all);
        Assert.Equal(emp.EmployeeId, all[0].EmployeeId);
    }
    
    [Fact]
    public async Task CreateEmployeeAsync_InvalidDates_ThrowsArgumentException()
    {
        var birth = DateOnly.Parse("2000-01-01");
        var invalidStart = DateOnly.Parse("1990-01-01");
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeService.CreateEmployeeAsync(birth, invalidStart));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeService.CreateEmployeeAsync(default, DateOnly.Parse("2020-01-01")));
    }
    
    [Fact]
    public async Task GetEmployeeByIdAsync_Existing_ReturnsEmployee()
    {
        var seed = new DimEmployee(42, DateOnly.Parse("1985-05-05"), DateOnly.Parse("2005-09-01"));
        await _accessObject.SeedEmployeeAsync(new[] { seed });

        var fetched = await _dimEmployeeService.GetEmployeeByIdAsync(42);

        Assert.Equal(seed.EmployeeId, fetched.EmployeeId);
        Assert.Equal(seed.BirthDate, fetched.BirthDate);
        Assert.Equal(seed.CareerStartDate, fetched.CareerStartDate);
    }
    
    [Fact]
    public async Task GetEmployeeByIdAsync_NotFound_ThrowsException()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployeeService.GetEmployeeByIdAsync(999));
        Assert.Contains("was not found", ex.Message);
    }
    
    [Fact]
    public async Task GetAllEmployeesAsync_Seeded_ReturnsAll()
    {
        var seeds = new[]
        {
            new DimEmployee(1, DateOnly.Parse("1991-01-01"), DateOnly.Parse("2011-01-01")),
            new DimEmployee(2, DateOnly.Parse("1992-02-02"), DateOnly.Parse("2012-02-02"))
        };
        await _accessObject.SeedEmployeeAsync(seeds);

        var list = (await _dimEmployeeService.GetAllEmployeesAsync()).ToList();

        Assert.Equal(2, list.Count);
        Assert.Contains(list, e => e.EmployeeId == 1);
        Assert.Contains(list, e => e.EmployeeId == 2);
    }
    
    [Fact]
    public async Task UpdateEmployeeAsync_Existing_UpdatesDates()
    {
        var original = new DimEmployee(5, DateOnly.Parse("1980-03-03"), DateOnly.Parse("2000-03-03"));
        await _accessObject.SeedEmployeeAsync(new[] { original });

        var newBirth = DateOnly.Parse("1980-04-04");
        var newStart = DateOnly.Parse("2000-04-04");
        var updated = await _dimEmployeeService.UpdateEmployeeAsync(5, newBirth, newStart);

        Assert.Equal(5, updated.EmployeeId);
        Assert.Equal(newBirth, updated.BirthDate);
        Assert.Equal(newStart, updated.CareerStartDate);
        
        var fetched = await _dimEmployeeService.GetEmployeeByIdAsync(5);
        Assert.Equal(newStart, fetched.CareerStartDate);
    }
    
    [Fact]
    public async Task UpdateEmployeeAsync_NotFound_ThrowsException()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployeeService.UpdateEmployeeAsync(123, DateOnly.Parse("1990-01-01"), DateOnly.Parse("2010-01-01")));
        Assert.Contains("not found", ex.Message);
    }
    
    [Fact]
    public async Task UpdateEmployeeAsync_InvalidParameters_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeService.UpdateEmployeeAsync(0, DateOnly.Parse("1990-01-01"), DateOnly.Parse("2010-01-01")));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeService.UpdateEmployeeAsync(1, DateOnly.Parse("2000-01-01"), DateOnly.Parse("1990-01-01")));
    }
    
    [Fact]
    public async Task DeleteEmployeeAsync_Existing_RemovesEmployee()
    {
        var seed = new DimEmployee(7, DateOnly.Parse("1970-07-07"), DateOnly.Parse("1990-07-07"));
        await _accessObject.SeedEmployeeAsync(new[] { seed });

        await _dimEmployeeService.DeleteEmployeeAsync(7);

        var all = (await _dimEmployeeService.GetAllEmployeesAsync()).ToList();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task DeleteEmployeeAsync_NotFound_ThrowsException()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployeeService.DeleteEmployeeAsync(888));
        Assert.Contains("not found", ex.Message);
    }
}