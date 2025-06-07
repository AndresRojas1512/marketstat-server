using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimIndustryFieldService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimIndustryFieldServiceUnitTests
{
    private readonly Mock<IDimIndustryFieldRepository> _dimIndustryFieldRepositoryMock;
    private readonly Mock<ILogger<DimIndustryFieldService>> _loggerMock;
    private readonly DimIndustryFieldService _dimIndustryFieldService;

    private DimIndustryField CreateTestIndustryField(int id, string code, string name)
    {
        return new DimIndustryField(id, code, name);
    }

    public DimIndustryFieldServiceUnitTests()
    {
        _dimIndustryFieldRepositoryMock = new Mock<IDimIndustryFieldRepository>();
        _loggerMock = new Mock<ILogger<DimIndustryFieldService>>();
        _dimIndustryFieldService = new DimIndustryFieldService(_dimIndustryFieldRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateIndustryFieldAsync_WithValidParameters_ReturnsNewField()
    {
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.AddIndustryFieldAsync(It.IsAny<DimIndustryField>()))
            .Callback<DimIndustryField>(f => f.IndustryFieldId = 1)
            .Returns(Task.CompletedTask);
        
        var result = await _dimIndustryFieldService.CreateIndustryFieldAsync("IT", "Information Technology");

        Assert.NotNull(result);
        Assert.Equal(1, result.IndustryFieldId);
        Assert.Equal("IT", result.IndustryFieldCode);
        Assert.Equal("Information Technology", result.IndustryFieldName);
        _dimIndustryFieldRepositoryMock.Verify(r =>
            r.AddIndustryFieldAsync(
                It.Is<DimIndustryField>(f =>
                    f.IndustryFieldCode == "IT" &&
                    f.IndustryFieldName == "Information Technology"
            )), Times.Once);
    }

    [Fact]
    public async Task CreateIndustryFieldAsync_WithEmptyCode_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimIndustryFieldService.CreateIndustryFieldAsync("", "Finance"));
    }
    
    [Fact]
    public async Task GetIndustryFieldByIdAsync_ExistingId_ReturnsField()
    {
        var expected = CreateTestIndustryField(42, "FIN", "Finance");
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.GetIndustryFieldByIdAsync(42))
            .ReturnsAsync(expected);
    
        var result = await _dimIndustryFieldService.GetIndustryFieldByIdAsync(42);
        
        Assert.Same(expected, result);
    }
    
    [Fact]
    public async Task GetIndustryFieldByIdAsync_NonExistingId_ThrowsNotFoundException()
    {
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.GetIndustryFieldByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new NotFoundException("not found"));
    
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimIndustryFieldService.GetIndustryFieldByIdAsync(99));
    }
    
    [Fact]
    public async Task GetAllIndustryFieldsAsync_ReturnsList()
    {
        var list = new List<DimIndustryField>
        {
            CreateTestIndustryField(1, "A_CODE", "A"),
            CreateTestIndustryField(2, "B_CODE", "B")
        };
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.GetAllIndustryFieldsAsync())
            .ReturnsAsync(list);
    
        var result = (await _dimIndustryFieldService.GetAllIndustryFieldsAsync()).ToList();
    
        Assert.Equal(2, result.Count);
        Assert.Equal(list, result);
    }

    [Fact]
    public async Task UpdateIndustryFieldAsync_WithValidParameters_ReturnsUpdated()
    {
        var existing = CreateTestIndustryField(7, "OLD", "Old Name");
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.GetIndustryFieldByIdAsync(7))
            .ReturnsAsync(existing);
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.UpdateIndustryFieldAsync(It.IsAny<DimIndustryField>()))
            .Returns(Task.CompletedTask);
    
        var updated = await _dimIndustryFieldService.UpdateIndustryFieldAsync(7, "NEW", "New Name");
    
        Assert.Equal(7, updated.IndustryFieldId);
        Assert.Equal("NEW", updated.IndustryFieldCode);
        Assert.Equal("New Name", updated.IndustryFieldName);
        _dimIndustryFieldRepositoryMock.Verify(r =>
            r.UpdateIndustryFieldAsync(
                It.Is<DimIndustryField>(f =>
                    f.IndustryFieldId == 7 &&
                    f.IndustryFieldCode == "NEW" &&
                    f.IndustryFieldName == "New Name"
            )), Times.Once);
    }
    
    [Fact]
    public async Task UpdateIndustryFieldAsync_NonExistingId_ThrowsNotFoundException()
    {
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.GetIndustryFieldByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new NotFoundException("not found"));
    
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimIndustryFieldService.UpdateIndustryFieldAsync(123, "CODE", "Name"));
    }
    
    [Fact]
    public async Task UpdateIndustryFieldAsync_WithEmptyName_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimIndustryFieldService.UpdateIndustryFieldAsync(1, "CODE", ""));
    }

    [Fact]
    public async Task DeleteIndustryFieldAsync_ExistingId_Completes()
    {
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.DeleteIndustryFieldAsync(5))
            .Returns(Task.CompletedTask);
    
        await _dimIndustryFieldService.DeleteIndustryFieldAsync(5);
    
        _dimIndustryFieldRepositoryMock.Verify(r => r.DeleteIndustryFieldAsync(5), Times.Once);
    }

    [Fact]
    public async Task DeleteIndustryFieldAsync_NonExistingId_ThrowsNotFoundException()
    {
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.DeleteIndustryFieldAsync(It.IsAny<int>()))
            .ThrowsAsync(new NotFoundException("not found"));
    
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimIndustryFieldService.DeleteIndustryFieldAsync(88));
    }
}