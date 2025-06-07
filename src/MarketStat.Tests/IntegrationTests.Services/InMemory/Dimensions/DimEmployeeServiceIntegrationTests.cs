using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Services.Dimensions.DimEmployeeService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimEmployeeServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimEmployeeService _dimEmployeeService;

    private DimEmployee CreateTestEmployeeInstance(
        int employeeId = 0,
        string refId = "TEST-REF-000",
        string birthDate = "1990-01-01",
        string careerStartDate = "2010-01-01",
        string? gender = "Male")
    {
        return new DimEmployee(
            employeeId,
            refId,
            DateOnly.Parse(birthDate),
            DateOnly.Parse(careerStartDate),
            gender
        );
    }

    public DimEmployeeServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimEmployeeService = _accessObject.DimEmployeeService;
    }

    public void Dispose() => _accessObject.Dispose();

    [Fact]
    public async Task CreateEmployeeAsync_ValidData_CreatesAndReturnsNewEmployee()
    {
        var refId = "EMP-001";
        var birthDate = DateOnly.Parse("1992-03-15");
        var careerStartDate = DateOnly.Parse("2014-07-01");
        var gender = "Female";
        
        var createdEmployee = await _dimEmployeeService.CreateEmployeeAsync(refId, birthDate, careerStartDate, gender);
        
        Assert.True(createdEmployee.EmployeeId > 0, "Database should generate a positive EmployeeId.");
        Assert.Equal(refId, createdEmployee.EmployeeRefId);
        Assert.Equal(birthDate, createdEmployee.BirthDate);
        Assert.Equal(careerStartDate, createdEmployee.CareerStartDate);
        Assert.Equal(gender, createdEmployee.Gender);

        // Verify persistence
        var fetched = await _dimEmployeeService.GetEmployeeByIdAsync(createdEmployee.EmployeeId);
        Assert.NotNull(fetched);
        Assert.Equal(refId, fetched.EmployeeRefId);
    }

    [Fact]
    public async Task CreateEmployeeAsync_InvalidData_ThrowsArgumentException()
    {
        var birthDate = DateOnly.Parse("2000-01-01");
        var careerStartDate = DateOnly.Parse("2020-01-01");
        
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeService.CreateEmployeeAsync("", birthDate, careerStartDate, "Male")
        );
        
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeService.CreateEmployeeAsync("EMP-002", birthDate, DateOnly.Parse("1999-01-01"), "Male")
        );
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_Existing_ReturnsEmployee()
    {
        var seedEmployee = CreateTestEmployeeInstance(employeeId: 42, refId: "EMP-42");
        await _accessObject.SeedEmployeeAsync(new[] { seedEmployee });
        
        var fetched = await _dimEmployeeService.GetEmployeeByIdAsync(42);
        
        Assert.NotNull(fetched);
        Assert.Equal(seedEmployee.EmployeeId, fetched.EmployeeId);
        Assert.Equal(seedEmployee.EmployeeRefId, fetched.EmployeeRefId);
        Assert.Equal(seedEmployee.Gender, fetched.Gender);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_NotFound_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployeeService.GetEmployeeByIdAsync(9999)
        );
    }

    [Fact]
    public async Task GetAllEmployeesAsync_Seeded_ReturnsAll()
    {
        var seeds = new[]
        {
            CreateTestEmployeeInstance(employeeId: 1, refId: "EMP-001"),
            CreateTestEmployeeInstance(employeeId: 2, refId: "EMP-002", gender: "Female")
        };
        await _accessObject.SeedEmployeeAsync(seeds);
        
        var list = (await _dimEmployeeService.GetAllEmployeesAsync()).ToList();
        
        Assert.Equal(2, list.Count);
        Assert.Contains(list, e => e.EmployeeRefId == "EMP-001");
        Assert.Contains(list, e => e.EmployeeRefId == "EMP-002");
    }

    [Fact]
    public async Task UpdateEmployeeAsync_Existing_PersistsChanges()
    {
        var original = CreateTestEmployeeInstance(employeeId: 5, refId: "EMP-005", gender: "Male");
        await _accessObject.SeedEmployeeAsync(new[] { original });

        var newRefId = "EMP-005-UPDATED";
        var newBirthDate = DateOnly.Parse("1981-04-04");
        var newCareerStartDate = DateOnly.Parse("2001-04-04");
        var newGender = "Other";
        
        var updated = await _dimEmployeeService.UpdateEmployeeAsync(5, newRefId, newBirthDate, newCareerStartDate, newGender);
        
        Assert.Equal(5, updated.EmployeeId);
        Assert.Equal(newRefId, updated.EmployeeRefId);
        Assert.Equal(newBirthDate, updated.BirthDate);
        Assert.Equal(newCareerStartDate, updated.CareerStartDate);
        Assert.Equal(newGender, updated.Gender);
        
        var fetched = await _dimEmployeeService.GetEmployeeByIdAsync(5);
        Assert.NotNull(fetched);
        Assert.Equal(newRefId, fetched.EmployeeRefId);
        Assert.Equal(newGender, fetched.Gender);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_NotFound_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployeeService.UpdateEmployeeAsync(123, "EMP-123", new DateOnly(1990,1,1), new DateOnly(2010,1,1), "Male")
        );
    }

    [Fact]
    public async Task DeleteEmployeeAsync_Existing_RemovesEmployee()
    {
        var seed = CreateTestEmployeeInstance(employeeId: 7, refId: "EMP-TO-DELETE");
        await _accessObject.SeedEmployeeAsync(new[] { seed });
        
        await _dimEmployeeService.DeleteEmployeeAsync(7);
        
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _dimEmployeeService.GetEmployeeByIdAsync(7)
        );
    }

    [Fact]
    public async Task DeleteEmployeeAsync_NotFound_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployeeService.DeleteEmployeeAsync(888)
        );
    }
}