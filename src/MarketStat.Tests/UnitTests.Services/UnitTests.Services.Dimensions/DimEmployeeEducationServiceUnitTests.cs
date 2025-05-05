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
    public async Task CreateEmployeeEducationAsync_ValidParameters_CallsRepositoryOnce()
    {
        const int empId = 1, eduId = 2;
        const short gradYear = 2020;
        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.AddEmployeeEducationAsync(It.IsAny<DimEmployeeEducation>()))
            .Returns(Task.CompletedTask);

        var result = await _dimEmployeeEducationService
            .CreateEmployeeEducationAsync(empId, eduId, gradYear);

        Assert.Equal(empId, result.EmployeeId);
        Assert.Equal(eduId, result.EducationId);
        Assert.Equal(gradYear, result.GraduationYear);

        _dimEmployeeEducationRepositoryMock.Verify(r =>
            r.AddEmployeeEducationAsync(
                It.Is<DimEmployeeEducation>(l =>
                    l.EmployeeId     == empId &&
                    l.EducationId    == eduId &&
                    l.GraduationYear == gradYear
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
    public async Task CreateEmployeeEducationAsync_RepositoryThrows_WrapsAndThrowsException()
    {
        const int empId = 1, eduId = 2;
        const short gradYear = 2020;
        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.AddEmployeeEducationAsync(It.IsAny<DimEmployeeEducation>()))
            .ThrowsAsync(new InvalidOperationException("db error"));

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployeeEducationService.CreateEmployeeEducationAsync(empId, eduId, gradYear));

        Assert.Equal($"Failed to create DimEmployeeEducation ({empId},{eduId}).", ex.Message);
    }
    
    [Fact]
    public async Task DeleteEmployeeEducationAsync_ValidParameters_CallsRepositoryOnce()
    {
        const int empId = 5, eduId = 7;
        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.DeleteEmployeeEducationAsync(empId, eduId))
            .Returns(Task.CompletedTask);

        await _dimEmployeeEducationService.DeleteEmployeeEducationAsync(empId, eduId);

        _dimEmployeeEducationRepositoryMock.Verify(r =>
            r.DeleteEmployeeEducationAsync(empId, eduId), Times.Once);
    }
    
    [Fact]
    public async Task DeleteEmployeeEducationAsync_RepositoryThrows_WrapsAndThrowsException()
    {
        const int empId = 3, eduId = 4;
        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.DeleteEmployeeEducationAsync(empId, eduId))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployeeEducationService.DeleteEmployeeEducationAsync(empId, eduId));

        Assert.Equal($"Could not remove education {eduId} from employee {empId}", ex.Message);
    }
    
    [Fact]
    public async Task GetEducationsByEmployeeIdAsync_ValidId_ReturnsLinks()
    {
        const int empId = 10;
        var fakeList = new List<DimEmployeeEducation>
        {
            new DimEmployeeEducation(empId, 1, 2020),
            new DimEmployeeEducation(empId, 2, 2021)
        };
        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.GetEducationsByEmployeeIdAsync(empId))
            .ReturnsAsync(fakeList);

        var result = await _dimEmployeeEducationService.GetEducationsByEmployeeIdAsync(empId);

        Assert.Equal(fakeList, result.ToList());
    }
    
    [Fact]
    public async Task GetEducationsByEmployeeIdAsync_NoLinks_ReturnsEmpty()
    {
        const int empId = 42;
        _dimEmployeeEducationRepositoryMock
            .Setup(r => r.GetEducationsByEmployeeIdAsync(empId))
            .ReturnsAsync(new List<DimEmployeeEducation>());

        var result = await _dimEmployeeEducationService.GetEducationsByEmployeeIdAsync(empId);

        Assert.Empty(result);
    }
}