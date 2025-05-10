using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployeeEducationService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimEmployeeEducationServiceUnitTests
{
    private readonly Mock<IDimEmployeeEducationRepository> _dimEmployeeEducationRepositoryMock;
    private readonly Mock<ILogger<DimEmployeeEducationService>> _loggerMock;
    private readonly DimEmployeeEducationService _dimEmployeeEducationService;

    public DimEmployeeEducationServiceUnitTests()
    {
        _dimEmployeeEducationRepositoryMock = new Mock<IDimEmployeeEducationRepository>();
        _loggerMock = new Mock<ILogger<DimEmployeeEducationService>>();
        _dimEmployeeEducationService = new DimEmployeeEducationService(_dimEmployeeEducationRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateEmployeeEducationAsync_ValidParameters_CallsRepositoryOnce()
    {
        const int empId    = 1;
        const int eduId    = 2;
        const short year   = 2020;

        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.AddEmployeeEducationAsync(It.IsAny<DimEmployeeEducation>()))
            .Returns(Task.CompletedTask);

        var result = await _dimEmployeeEducationService
            .CreateEmployeeEducationAsync(empId, eduId, year);

        Assert.Equal(empId,    result.EmployeeId);
        Assert.Equal(eduId,    result.EducationId);
        Assert.Equal(year,     result.GraduationYear);

        _dimEmployeeEducationRepositoryMock.Verify(r =>
            r.AddEmployeeEducationAsync(
                It.Is<DimEmployeeEducation>(l =>
                    l.EmployeeId     == empId &&
                    l.EducationId    == eduId &&
                    l.GraduationYear == year
                )), Times.Once);
    }

    [Fact]
    public async Task CreateEmployeeEducationAsync_InvalidEmployeeId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeEducationService.CreateEmployeeEducationAsync(0, 1, 2020));
    }

    [Fact]
    public async Task CreateEmployeeEducationAsync_InvalidEducationId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeEducationService.CreateEmployeeEducationAsync(1, 0, 2020));
    }

    [Fact]
    public async Task CreateEmployeeEducationAsync_RepositoryThrowsConflictException_PropagatesConflictException()
    {
        const int empId = 1, eduId = 2;
        const short year = 2020;
        var message = $"Conflict linking employee {empId} & education {eduId}";

        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.AddEmployeeEducationAsync(It.IsAny<DimEmployeeEducation>()))
            .ThrowsAsync(new ConflictException(message));

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _dimEmployeeEducationService.CreateEmployeeEducationAsync(empId, eduId, year)
        );

        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public async Task CreateEmployeeEducationAsync_RepositoryThrowsNotFoundException_PropagatesNotFoundException()
    {
        const int empId = 1, eduId = 2;
        const short year = 2020;
        var message = $"Referenced FK not found for employee {empId} or education {eduId}";

        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.AddEmployeeEducationAsync(It.IsAny<DimEmployeeEducation>()))
            .ThrowsAsync(new NotFoundException(message));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployeeEducationService.CreateEmployeeEducationAsync(empId, eduId, year)
        );

        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public async Task GetEmployeeEducationAsync_NotFound_ThrowsNotFoundException()
    {
        const int empId = 3, eduId = 4;
        var message = $"EmployeeEducation ({empId}, {eduId}) not found.";

        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.GetEmployeeEducationAsync(empId, eduId))
            .ThrowsAsync(new NotFoundException(message));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployeeEducationService.GetEmployeeEducationAsync(empId, eduId)
        );

        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public async Task GetEducationsByEmployeeIdAsync_ValidId_ReturnsLinks()
    {
        const int empId = 10;
        var fakeList = new List<DimEmployeeEducation>
        {
            new(empId, 1, 2020),
            new(empId, 2, 2021)
        };
        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.GetEducationsByEmployeeIdAsync(empId))
            .ReturnsAsync(fakeList);

        var result = (await _dimEmployeeEducationService.GetEducationsByEmployeeIdAsync(empId)).ToList();

        Assert.Equal(fakeList, result);
    }

    [Fact]
    public async Task GetEducationsByEmployeeIdAsync_RepositoryThrows_PropagatesException()
    {
        const int empId = 42;
        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.GetEducationsByEmployeeIdAsync(empId))
            .ThrowsAsync(new Exception("db error"));

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployeeEducationService.GetEducationsByEmployeeIdAsync(empId)
        );

        Assert.Equal($"No Education found for Employee {empId}.", ex.Message);
    }

    [Fact]
    public async Task GetAllEmployeeEducationsAsync_ReturnsAll()
    {
        var allLinks = new List<DimEmployeeEducation>
        {
            new(1,1,2019), new(2,2,2020)
        };
        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.GetAllEmployeeEducationsAsync())
            .ReturnsAsync(allLinks);

        var result = (await _dimEmployeeEducationService.GetAllEmployeeEducationsAsync()).ToList();

        Assert.Equal(allLinks, result);
    }

    [Fact]
    public async Task UpdateEmployeeEducationAsync_ValidParameters_UpdatesAndReturns()
    {
        const int empId = 5, eduId = 6;
        const short year = 2022;
        var existing = new DimEmployeeEducation(empId, eduId, 2020);

        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.GetEmployeeEducationAsync(empId, eduId))
            .ReturnsAsync(existing);
        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.UpdateEmployeeEducationAsync(It.IsAny<DimEmployeeEducation>()))
            .Returns(Task.CompletedTask);

        var updated = await _dimEmployeeEducationService.UpdateEmployeeEducationAsync(empId, eduId, year);

        Assert.Equal(year, updated.GraduationYear);
        _dimEmployeeEducationRepositoryMock.Verify(r =>
            r.UpdateEmployeeEducationAsync(
                It.Is<DimEmployeeEducation>(l => 
                    l.EmployeeId     == empId &&
                    l.EducationId    == eduId &&
                    l.GraduationYear == year
                )), Times.Once);
    }

    [Fact]
    public async Task UpdateEmployeeEducationAsync_NotFound_ThrowsNotFoundException()
    {
        const int empId = 5, eduId = 6;
        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.GetEmployeeEducationAsync(empId, eduId))
            .ThrowsAsync(new NotFoundException($"Cannot update: link ({empId},{eduId})"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployeeEducationService.UpdateEmployeeEducationAsync(empId, eduId, 2023)
        );
    }

    [Fact]
    public async Task DeleteEmployeeEducationAsync_ValidParameters_CallsRepositoryOnce()
    {
        const int empId = 7, eduId = 8;
        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.DeleteEmployeeEducationAsync(empId, eduId))
            .Returns(Task.CompletedTask);

        await _dimEmployeeEducationService.DeleteEmployeeEducationAsync(empId, eduId);

        _dimEmployeeEducationRepositoryMock.Verify(r =>
            r.DeleteEmployeeEducationAsync(empId, eduId), Times.Once);
    }

    [Fact]
    public async Task DeleteEmployeeEducationAsync_NotFound_ThrowsNotFoundException()
    {
        const int empId = 9, eduId = 10;
        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.DeleteEmployeeEducationAsync(empId, eduId))
            .ThrowsAsync(new NotFoundException($"Cannot delete: link ({empId},{eduId})"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployeeEducationService.DeleteEmployeeEducationAsync(empId, eduId)
        );
    }
}