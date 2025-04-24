using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployeeService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimEmployeeServiceUnitTests
{
    private readonly Mock<IDimEmployeeRepository> _dimEmployeeRepositoryMock;
    private readonly Mock<ILogger<DimEmployeeService>> _loggerMock;
    private readonly DimEmployeeService _dimEmployeeService;

    public DimEmployeeServiceUnitTests()
    {
        _dimEmployeeRepositoryMock = new Mock<IDimEmployeeRepository>();
        _loggerMock = new Mock<ILogger<DimEmployeeService>>();
        _dimEmployeeService = new DimEmployeeService(_dimEmployeeRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateEmployeeAsync_EmptyRepo_CreatesWithId1()
    {
        // Arrange
        _dimEmployeeRepositoryMock
            .Setup(r => r.GetAllEmployeesAsync())
            .ReturnsAsync(Array.Empty<DimEmployee>());

        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var emp = await _dimEmployeeService.CreateEmployeeAsync(birthDate);

        // Assert
        Assert.Equal(1, emp.EmployeeId);
        Assert.Equal(birthDate, emp.BirthDate);
        _dimEmployeeRepositoryMock.Verify(r => r.AddEmployeeAsync(
            It.Is<DimEmployee>(e => e.EmployeeId == 1 && e.BirthDate == birthDate)
        ), Times.Once);
    }
    
    [Fact]
    public async Task CreateEmployeeAsync_NonEmptyRepo_IncrementsId()
    {
        // Arrange
        var existing = new List<DimEmployee> { new DimEmployee(5, DateOnly.FromDateTime(DateTime.UtcNow)) };
        _dimEmployeeRepositoryMock
            .Setup(r => r.GetAllEmployeesAsync())
            .ReturnsAsync(existing);

        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var emp = await _dimEmployeeService.CreateEmployeeAsync(birthDate);

        // Assert
        Assert.Equal(6, emp.EmployeeId);
        _dimEmployeeRepositoryMock.Verify(r => r.AddEmployeeAsync(
            It.Is<DimEmployee>(e => e.EmployeeId == 6)
        ), Times.Once);
    }
    
    [Fact]
    public async Task CreateEmployeeAsync_FutureBirthDate_ThrowsArgumentException()
    {
        var future = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeService.CreateEmployeeAsync(future)
        );
    }
    
    [Fact]
    public async Task CreateEmployeeAsync_RepositoryThrows_WrapsException()
    {
        // Arrange
        _dimEmployeeRepositoryMock
            .Setup(r => r.GetAllEmployeesAsync())
            .ReturnsAsync(Array.Empty<DimEmployee>());
        _dimEmployeeRepositoryMock
            .Setup(r => r.AddEmployeeAsync(It.IsAny<DimEmployee>()))
            .ThrowsAsync(new InvalidOperationException("db fail"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployeeService.CreateEmployeeAsync(DateOnly.FromDateTime(DateTime.UtcNow))
        );
        Assert.Contains("Could not create employee 1", ex.Message);
    }
    
    [Fact]
    public async Task GetEmployeeByIdAsync_Found_ReturnsEmployee()
    {
        var emp = new DimEmployee(2, DateOnly.FromDateTime(DateTime.UtcNow));
        _dimEmployeeRepositoryMock
            .Setup(r => r.GetEmployeeByIdAsync(2))
            .ReturnsAsync(emp);

        var result = await _dimEmployeeService.GetEmployeeByIdAsync(2);

        Assert.Same(emp, result);
    }
    
    [Fact]
    public async Task GetEmployeeByIdAsync_NotFound_WrapsException()
    {
        _dimEmployeeRepositoryMock
            .Setup(r => r.GetEmployeeByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployeeService.GetEmployeeByIdAsync(99)
        );
        Assert.Contains("Employee 99 was not found", ex.Message);
    }
    
    [Fact]
    public async Task GetAllEmployeesAsync_ReturnsList()
    {
        var list = new List<DimEmployee>
        {
            new DimEmployee(1, DateOnly.FromDateTime(DateTime.UtcNow))
        };
        _dimEmployeeRepositoryMock
            .Setup(r => r.GetAllEmployeesAsync())
            .ReturnsAsync(list);

        var result = await _dimEmployeeService.GetAllEmployeesAsync();

        Assert.Equal(list, result.ToList());
    }
    
    [Fact]
    public async Task UpdateEmployeeAsync_ValidParameters_UpdatesAndReturns()
    {
        // Arrange
        var orig = new DimEmployee(3, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)));
        _dimEmployeeRepositoryMock
            .Setup(r => r.GetEmployeeByIdAsync(3))
            .ReturnsAsync(orig);

        var newDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2));

        // Act
        var updated = await _dimEmployeeService.UpdateEmployeeAsync(3, newDate);

        // Assert
        Assert.Equal(newDate, updated.BirthDate);
        _dimEmployeeRepositoryMock.Verify(r =>
                r.UpdateEmployeeAsync(It.Is<DimEmployee>(e => e.EmployeeId == 3 && e.BirthDate == newDate)),
            Times.Once
        );
    }
    
    [Fact]
    public async Task UpdateEmployeeAsync_InvalidId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeService.UpdateEmployeeAsync(0, DateOnly.FromDateTime(DateTime.UtcNow))
        );
    }
    
    [Fact]
    public async Task UpdateEmployeeAsync_ReposNotFound_WrapsException()
    {
        _dimEmployeeRepositoryMock
            .Setup(r => r.GetEmployeeByIdAsync(5))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployeeService.UpdateEmployeeAsync(5, DateOnly.FromDateTime(DateTime.UtcNow))
        );
        Assert.Contains("Cannot update: employee 5 not found", ex.Message);
    }
    
    [Fact]
    public async Task DeleteEmployeeAsync_ValidId_CallsRepository()
    {
        await _dimEmployeeService.DeleteEmployeeAsync(4);
        _dimEmployeeRepositoryMock.Verify(r => r.DeleteEmployeeAsync(4), Times.Once);
    }
    
    [Fact]
    public async Task DeleteEmployeeAsync_RepositoryThrows_WrapsException()
    {
        _dimEmployeeRepositoryMock
            .Setup(r => r.DeleteEmployeeAsync(7))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployeeService.DeleteEmployeeAsync(7)
        );
        Assert.Contains("Cannot delete: employee 7 not found", ex.Message);
    }
}