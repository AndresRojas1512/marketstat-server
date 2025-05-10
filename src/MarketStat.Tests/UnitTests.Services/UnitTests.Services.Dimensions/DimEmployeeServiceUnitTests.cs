using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
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
    public async Task CreateEmployeeAsync_ValidDates_AssignsId()
    {
        var birthDate       = new DateOnly(1990, 1, 1);
        var careerStartDate = new DateOnly(2010, 5, 20);

        _dimEmployeeRepositoryMock
            .Setup(r => r.AddEmployeeAsync(It.IsAny<DimEmployee>()))
            .Callback<DimEmployee>(e => e.EmployeeId = 1)
            .Returns(Task.CompletedTask);

        var result = await _dimEmployeeService.CreateEmployeeAsync(birthDate, careerStartDate);

        Assert.Equal(1, result.EmployeeId);
        Assert.Equal(birthDate, result.BirthDate);
        Assert.Equal(careerStartDate, result.CareerStartDate);

        _dimEmployeeRepositoryMock.Verify(r =>
            r.AddEmployeeAsync(It.Is<DimEmployee>(e =>
                e.EmployeeId      == 1 &&
                e.BirthDate       == birthDate &&
                e.CareerStartDate == careerStartDate
            )), Times.Once);
    }

    [Fact]
    public async Task CreateEmployeeAsync_FutureBirthDate_ThrowsArgumentException()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var today    = DateOnly.FromDateTime(DateTime.UtcNow);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeService.CreateEmployeeAsync(tomorrow, today)
        );
    }

    [Fact]
    public async Task CreateEmployeeAsync_RepositoryThrows_WrapsException()
    {
        var birthDate       = new DateOnly(1990, 1, 1);
        var careerStartDate = new DateOnly(2010, 1, 1);

        _dimEmployeeRepositoryMock
            .Setup(r => r.AddEmployeeAsync(It.IsAny<DimEmployee>()))
            .ThrowsAsync(new Exception("db fail"));

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployeeService.CreateEmployeeAsync(birthDate, careerStartDate)
        );

        Assert.Contains("Could not create employee 0", ex.Message);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_Existing_ReturnsEmployee()
    {
        var emp = new DimEmployee(2, new DateOnly(1985, 2, 2), new DateOnly(2005, 2, 2));
        _dimEmployeeRepositoryMock
            .Setup(r => r.GetEmployeeByIdAsync(2))
            .ReturnsAsync(emp);

        var result = await _dimEmployeeService.GetEmployeeByIdAsync(2);

        Assert.Same(emp, result);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_NotFound_ThrowsNotFoundException()
    {
        _dimEmployeeRepositoryMock
            .Setup(r => r.GetEmployeeByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new NotFoundException("not found"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployeeService.GetEmployeeByIdAsync(99)
        );
    }

    [Fact]
    public async Task GetAllEmployeesAsync_ReturnsList()
    {
        var list = new List<DimEmployee>
        {
            new(1, new DateOnly(1990,1,1), new DateOnly(2010,1,1))
        };
        _dimEmployeeRepositoryMock
            .Setup(r => r.GetAllEmployeesAsync())
            .ReturnsAsync(list);

        var result = (await _dimEmployeeService.GetAllEmployeesAsync()).ToList();

        Assert.Equal(list, result);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_Valid_UpdatesAndReturns()
    {
        var existing = new DimEmployee(3, new DateOnly(1980,1,1), new DateOnly(2000,1,1));
        _dimEmployeeRepositoryMock
            .Setup(r => r.GetEmployeeByIdAsync(3))
            .ReturnsAsync(existing);
        _dimEmployeeRepositoryMock
            .Setup(r => r.UpdateEmployeeAsync(It.IsAny<DimEmployee>()))
            .Returns(Task.CompletedTask);

        var newBirth = new DateOnly(1981,2,2);
        var newCareer = new DateOnly(2001,2,2);

        var updated = await _dimEmployeeService.UpdateEmployeeAsync(3, newBirth, newCareer);

        Assert.Equal(newBirth, updated.BirthDate);
        Assert.Equal(newCareer, updated.CareerStartDate);

        _dimEmployeeRepositoryMock.Verify(r =>
            r.UpdateEmployeeAsync(It.Is<DimEmployee>(e =>
                e.EmployeeId      == 3 &&
                e.BirthDate       == newBirth &&
                e.CareerStartDate == newCareer
            )), Times.Once);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_InvalidId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeService.UpdateEmployeeAsync(0, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow))
        );
    }

    [Fact]
    public async Task UpdateEmployeeAsync_NotFound_ThrowsNotFoundException()
    {
        _dimEmployeeRepositoryMock
            .Setup(r => r.GetEmployeeByIdAsync(5))
            .ThrowsAsync(new NotFoundException("not found"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployeeService.UpdateEmployeeAsync(5, new DateOnly(1990,1,1), new DateOnly(2010,1,1))
        );
    }

    [Fact]
    public async Task DeleteEmployeeAsync_ValidId_CallsRepository()
    {
        _dimEmployeeRepositoryMock
            .Setup(r => r.DeleteEmployeeAsync(4))
            .Returns(Task.CompletedTask);

        await _dimEmployeeService.DeleteEmployeeAsync(4);

        _dimEmployeeRepositoryMock.Verify(r =>
            r.DeleteEmployeeAsync(4), Times.Once);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_NotFound_ThrowsNotFoundException()
    {
        _dimEmployeeRepositoryMock
            .Setup(r => r.DeleteEmployeeAsync(7))
            .ThrowsAsync(new NotFoundException("not found"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployeeService.DeleteEmployeeAsync(7)
        );
    }
}