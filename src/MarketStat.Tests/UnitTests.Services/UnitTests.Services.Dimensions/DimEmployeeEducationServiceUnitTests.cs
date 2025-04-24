using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
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
    public async Task AddEmployeeEducationAsync_ValidParameters_CallsRepositoryOnce()
    {
        // Arrange
        const int empId = 1, eduId = 2;

        // Act
        await _dimEmployeeEducationService.AddEmployeeEducationAsync(empId, eduId);

        // Assert
        _dimEmployeeEducationRepositoryMock.Verify(r =>
                r.AddEmployeeEducationAsync(
                    It.Is<DimEmployeeEducation>(l => l.EmployeeId == empId && l.EducationId == eduId)
                ),
            Times.Once
        );
    }
    
    [Fact]
    public async Task AddEmployeeEducationAsync_InvalidEmployeeId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeEducationService.AddEmployeeEducationAsync(0, 1)
        );
    }
    
    [Fact]
    public async Task AddEmployeeEducationAsync_InvalidEducationId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeEducationService.AddEmployeeEducationAsync(1, 0)
        );
    }
    
    [Fact]
    public async Task AddEmployeeEducationAsync_RepositoryThrows_WrapsAndThrowsException()
    {
        // Arrange
        const int empId = 1, eduId = 2;
        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.AddEmployeeEducationAsync(It.IsAny<DimEmployeeEducation>()))
            .ThrowsAsync(new InvalidOperationException("db error"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployeeEducationService.AddEmployeeEducationAsync(empId, eduId)
        );
        Assert.Contains($"Could not add education {eduId} to employee {empId}", ex.Message);
    }
    
    [Fact]
    public async Task RemoveEmployeeEducationAsync_ValidParameters_CallsRepositoryOnce()
    {
        // Arrange
        const int empId = 5, eduId = 7;

        // Act
        await _dimEmployeeEducationService.RemoveEmployeeEducationAsync(empId, eduId);

        // Assert
        _dimEmployeeEducationRepositoryMock.Verify(r =>
                r.RemoveEmployeeEducationAsync(empId, eduId),
            Times.Once
        );
    }
    
    [Fact]
    public async Task RemoveEmployeeEducationAsync_InvalidEmployeeId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeEducationService.RemoveEmployeeEducationAsync(0, 1)
        );
    }
    
    [Fact]
    public async Task RemoveEmployeeEducationAsync_InvalidEducationId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployeeEducationService.RemoveEmployeeEducationAsync(1, 0)
        );
    }
    
    [Fact]
    public async Task RemoveEmployeeEducationAsync_RepositoryThrows_WrapsAndThrowsException()
    {
        // Arrange
        const int empId = 3, eduId = 4;
        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.RemoveEmployeeEducationAsync(empId, eduId))
            .ThrowsAsync(new KeyNotFoundException("not found"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployeeEducationService.RemoveEmployeeEducationAsync(empId, eduId)
        );
        Assert.Contains($"Could not remove education {eduId} from employee {empId}", ex.Message);
    }
    
    [Fact]
    public async Task GetEducationsByEmployeeIdAsync_ValidId_ReturnsLinks()
    {
        // Arrange
        const int empId = 10;
        var fakeList = new[]
        {
            new DimEmployeeEducation(empId, 1),
            new DimEmployeeEducation(empId, 2)
        };
        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.GetEducationsByEmployeeIdAsync(empId))
            .ReturnsAsync(fakeList);

        // Act
        var result = await _dimEmployeeEducationService.GetEducationsByEmployeeIdAsync(empId);

        // Assert
        Assert.Equal(fakeList, result.ToList());
    }
    
    [Fact]
    public async Task GetEducationsByEmployeeIdAsync_InvalidId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            Task.FromResult(_dimEmployeeEducationService.GetEducationsByEmployeeIdAsync(0))
        );
    }
}