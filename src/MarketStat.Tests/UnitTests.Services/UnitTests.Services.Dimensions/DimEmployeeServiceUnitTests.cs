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

    private (string refId, DateOnly birthDate, DateOnly careerStartDate, string? gender) GetDefaultEmployeeParams(string refId = "EMP-TEST-001")
    {
        return (
            refId: refId,
            birthDate: new DateOnly(1990, 1, 1),
            careerStartDate: new DateOnly(2010, 5, 20),
            gender: "Female"
        );
    }

    public DimEmployeeServiceUnitTests()
    {
        _dimEmployeeRepositoryMock = new Mock<IDimEmployeeRepository>();
        _loggerMock = new Mock<ILogger<DimEmployeeService>>();
        _dimEmployeeService = new DimEmployeeService(_dimEmployeeRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ValidParameters_AssignsIdAndReturns()
    {
        var defaultParams = GetDefaultEmployeeParams();
        var expectedEmployeeId = 1;

        _dimEmployeeRepositoryMock.Setup(r => r.AddEmployeeAsync(It.IsAny<DimEmployee>()))
             .Callback<DimEmployee>(e => e.EmployeeId = expectedEmployeeId)
             .Returns(Task.CompletedTask);
        
        var result = await _dimEmployeeService.CreateEmployeeAsync(
            defaultParams.refId, defaultParams.birthDate, defaultParams.careerStartDate, defaultParams.gender
        );
        
        Assert.NotNull(result);
        Assert.Equal(expectedEmployeeId, result.EmployeeId);
        Assert.Equal(defaultParams.refId, result.EmployeeRefId);
        Assert.Equal(defaultParams.birthDate, result.BirthDate);
        Assert.Equal(defaultParams.careerStartDate, result.CareerStartDate);
        Assert.Equal(defaultParams.gender, result.Gender);

        _dimEmployeeRepositoryMock.Verify(r =>
            r.AddEmployeeAsync(It.Is<DimEmployee>(e =>
                e.EmployeeRefId == defaultParams.refId &&
                e.BirthDate == defaultParams.birthDate &&
                e.CareerStartDate == defaultParams.careerStartDate &&
                e.Gender == defaultParams.gender
        )), Times.Once);
    }

    [Fact]
    public async Task CreateEmployeeAsync_RepositoryThrowsConflict_ThrowsConflictException()
    {
        var defaultParams = GetDefaultEmployeeParams();
        _dimEmployeeRepositoryMock
            .Setup(r => r.AddEmployeeAsync(It.IsAny<DimEmployee>()))
            .ThrowsAsync(new ConflictException("Duplicate Ref ID"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _dimEmployeeService.CreateEmployeeAsync(
                defaultParams.refId, defaultParams.birthDate, defaultParams.careerStartDate, defaultParams.gender
            )
        );
    }

    [Fact]
    public async Task CreateEmployeeAsync_InvalidRefId_ThrowsArgumentException()
    {
        var defaultParams = GetDefaultEmployeeParams(refId: "");
        
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeService.CreateEmployeeAsync(
                defaultParams.refId, defaultParams.birthDate, defaultParams.careerStartDate, defaultParams.gender
            )
        );
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_Existing_ReturnsEmployee()
    {
        var defaultParams = GetDefaultEmployeeParams();
        var expectedEmployee = new DimEmployee(2, defaultParams.refId, defaultParams.birthDate, defaultParams.careerStartDate, defaultParams.gender);
        
        _dimEmployeeRepositoryMock
            .Setup(r => r.GetEmployeeByIdAsync(2))
            .ReturnsAsync(expectedEmployee);

        var result = await _dimEmployeeService.GetEmployeeByIdAsync(2);

        Assert.Same(expectedEmployee, result);
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
            new DimEmployee(1, "EMP-001", new DateOnly(1990,1,1), new DateOnly(2010,1,1), "Male"),
            new DimEmployee(2, "EMP-002", new DateOnly(1992,2,2), new DateOnly(2012,2,2), "Female")
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
        var existing = new DimEmployee(3, "EMP-OLD", new DateOnly(1980,1,1), new DateOnly(2000,1,1), "Male");
        _dimEmployeeRepositoryMock
            .Setup(r => r.GetEmployeeByIdAsync(3))
            .ReturnsAsync(existing);
        _dimEmployeeRepositoryMock
            .Setup(r => r.UpdateEmployeeAsync(It.IsAny<DimEmployee>()))
            .Returns(Task.CompletedTask);

        var newParams = GetDefaultEmployeeParams("EMP-NEW");

        var updated = await _dimEmployeeService.UpdateEmployeeAsync(3, newParams.refId, newParams.birthDate, newParams.careerStartDate, newParams.gender);

        Assert.Equal(newParams.birthDate, updated.BirthDate);
        Assert.Equal(newParams.careerStartDate, updated.CareerStartDate);
        Assert.Equal(newParams.refId, updated.EmployeeRefId);
        Assert.Equal(newParams.gender, updated.Gender);

        _dimEmployeeRepositoryMock.Verify(r =>
            r.UpdateEmployeeAsync(It.Is<DimEmployee>(e =>
                e.EmployeeId      == 3 &&
                e.EmployeeRefId   == newParams.refId &&
                e.BirthDate       == newParams.birthDate
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_InvalidId_ThrowsArgumentException()
    {
        var defaultParams = GetDefaultEmployeeParams();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeService.UpdateEmployeeAsync(0, defaultParams.refId, defaultParams.birthDate, defaultParams.careerStartDate, defaultParams.gender)
        );
    }

    [Fact]
    public async Task UpdateEmployeeAsync_NotFound_ThrowsNotFoundException()
    {
        var defaultParams = GetDefaultEmployeeParams();
        _dimEmployeeRepositoryMock
            .Setup(r => r.GetEmployeeByIdAsync(5))
            .ThrowsAsync(new NotFoundException("not found"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployeeService.UpdateEmployeeAsync(5, defaultParams.refId, defaultParams.birthDate, defaultParams.careerStartDate, defaultParams.gender)
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