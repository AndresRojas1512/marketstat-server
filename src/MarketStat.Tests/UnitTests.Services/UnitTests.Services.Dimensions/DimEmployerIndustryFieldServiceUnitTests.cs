using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerIndustryFieldService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimEmployerIndustryFieldServiceUnitTests
{
    private readonly Mock<IDimEmployerIndustryFieldRepository> _dimEmployerIndustryFieldRepositoryMock;
    private readonly Mock<ILogger<DimEmployerIndustryFieldService>> _loggerMock;
    private readonly DimEmployerIndustryFieldService _dimEmployerIndustryFieldService;

    public DimEmployerIndustryFieldServiceUnitTests()
    {
        _dimEmployerIndustryFieldRepositoryMock = new Mock<IDimEmployerIndustryFieldRepository>();
        _loggerMock = new Mock<ILogger<DimEmployerIndustryFieldService>>();
        _dimEmployerIndustryFieldService =
            new DimEmployerIndustryFieldService(_dimEmployerIndustryFieldRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateEmployerIndustryFieldAsync_RepositoryThrows_WrapsAndThrowsException()
    {
        const int empId = 4, indId = 6;
        _dimEmployerIndustryFieldRepositoryMock
            .Setup(r => r.AddEmployerIndustryFieldAsync(It.IsAny<DimEmployerIndustryField>()))
            .ThrowsAsync(new InvalidOperationException("duplicate"));

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployerIndustryFieldService.CreateEmployerIndustryFieldAsync(empId, indId));

        Assert.Equal($"Link ({empId}, {indId}) already exists.", ex.Message);
    }
    
    [Fact]
    public async Task GetEmployerIndustryFieldAsync_Existing_ReturnsLink()
    {
        var expected = new DimEmployerIndustryField(7, 8);
        _dimEmployerIndustryFieldRepositoryMock
            .Setup(r => r.GetEmployerIndustryFieldAsync(7, 8))
            .ReturnsAsync(expected);

        var actual = await _dimEmployerIndustryFieldService.GetEmployerIndustryFieldAsync(7, 8);

        Assert.Same(expected, actual);
    }
    
    [Fact]
    public async Task GetEmployerIndustryFieldAsync_NotFound_ThrowsException()
    {
        _dimEmployerIndustryFieldRepositoryMock
            .Setup(r => r.GetEmployerIndustryFieldAsync(1, 2))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployerIndustryFieldService.GetEmployerIndustryFieldAsync(1, 2));
        Assert.Equal("Link EmployerIndustryField (1, 2) not found.", ex.Message);
    }
    
    [Fact]
    public async Task GetIndustryFieldsByEmployerIdAsync_ReturnsLinks()
    {
        const int empId = 9;
        var list = new[]
        {
            new DimEmployerIndustryField(empId, 10),
            new DimEmployerIndustryField(empId, 11)
        };
        _dimEmployerIndustryFieldRepositoryMock
            .Setup(r => r.GetIndustryFieldsByEmployerIdAsync(empId))
            .ReturnsAsync(list);

        var result = await _dimEmployerIndustryFieldService.GetIndustryFieldsByEmployerIdAsync(empId);

        Assert.Equal(list, result.ToList());
    }
    
    [Fact]
    public async Task GetEmployersByIndustryFieldIdAsync_ReturnsLinks()
    {
        const int indId = 12;
        var list = new[]
        {
            new DimEmployerIndustryField(20, indId),
            new DimEmployerIndustryField(21, indId)
        };
        _dimEmployerIndustryFieldRepositoryMock
            .Setup(r => r.GetEmployersByIndustryFieldIdAsync(indId))
            .ReturnsAsync(list);

        var result = await _dimEmployerIndustryFieldService.GetEmployersByIndustryFieldIdAsync(indId);

        Assert.Equal(list, result.ToList());
    }
    
    [Fact]
    public async Task GetAllEmployerIndustryFieldsAsync_ReturnsAllLinks()
    {
        var list = new[]
        {
            new DimEmployerIndustryField(1, 2),
            new DimEmployerIndustryField(3, 4),
            new DimEmployerIndustryField(5, 6)
        };
        _dimEmployerIndustryFieldRepositoryMock
            .Setup(r => r.GetAllEmployerIndustryFieldsAsync())
            .ReturnsAsync(list);

        var result = await _dimEmployerIndustryFieldService.GetAllEmployerIndustryFieldsAsync();

        Assert.Equal(list, result.ToList());
    }
    
    [Fact]
    public async Task DeleteEmployerIndustryFieldAsync_Valid_CallsRepository()
    {
        const int empId = 13, indId = 14;
        _dimEmployerIndustryFieldRepositoryMock
            .Setup(r => r.DeleteEmployerIndustryFieldAsync(empId, indId))
            .Returns(Task.CompletedTask);

        await _dimEmployerIndustryFieldService.DeleteEmployerIndustryFieldAsync(empId, indId);

        _dimEmployerIndustryFieldRepositoryMock.Verify(r =>
            r.DeleteEmployerIndustryFieldAsync(empId, indId), Times.Once);
    }
    
    [Fact]
    public async Task DeleteEmployerIndustryFieldAsync_NotFound_ThrowsException()
    {
        const int empId = 15, indId = 16;
        _dimEmployerIndustryFieldRepositoryMock
            .Setup(r => r.DeleteEmployerIndustryFieldAsync(empId, indId))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployerIndustryFieldService.DeleteEmployerIndustryFieldAsync(empId, indId));
        Assert.Equal($"Cannot delete EmployerIndustryField link ({empId}, {indId}).", ex.Message);
    }
}