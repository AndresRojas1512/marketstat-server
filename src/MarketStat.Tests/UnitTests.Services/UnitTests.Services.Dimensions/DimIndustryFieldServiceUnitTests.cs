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

    public DimIndustryFieldServiceUnitTests()
    {
        _dimIndustryFieldRepositoryMock = new Mock<IDimIndustryFieldRepository>();
        _loggerMock = new Mock<ILogger<DimIndustryFieldService>>();
        _dimIndustryFieldService = new DimIndustryFieldService(_dimIndustryFieldRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateIndustryFieldAsync_WithValidName_ReturnsNewField()
    {
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.AddIndustryFieldAsync(It.IsAny<DimIndustryField>()))
            .Callback<DimIndustryField>(f => f.IndustryFieldId = 1)
            .Returns(Task.CompletedTask);

        var result = await _dimIndustryFieldService.CreateIndustryFieldAsync("Tech");
    
        Assert.NotNull(result);
        Assert.Equal(1, result.IndustryFieldId);
        Assert.Equal("Tech", result.IndustryFieldName);
        _dimIndustryFieldRepositoryMock.Verify(r =>
            r.AddIndustryFieldAsync(
                It.Is<DimIndustryField>(f =>
                    f.IndustryFieldId == 1 &&
                    f.IndustryFieldName == "Tech"
                )), Times.Once);
    }
    
    [Fact]
    public async Task CreateIndustryFieldAsync_WithEmptyName_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimIndustryFieldService.CreateIndustryFieldAsync(""));
    }
        
    [Fact]
    public async Task GetIndustryFieldByIdAsync_ExistingId_ReturnsField()
    {
        var existing = new DimIndustryField(42, "Finance");
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.GetIndustryFieldByIdAsync(42))
            .ReturnsAsync(existing);
        
        var result = await _dimIndustryFieldService.GetIndustryFieldByIdAsync(42);
        
        Assert.Same(existing, result);
    }
        
    [Fact]
    public async Task GetIndustryFieldByIdAsync_NonExistingId_ThrowsNotFoundException()
    {
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.GetIndustryFieldByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new NotFoundException("Industry field with ID 99 not found."));
        
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimIndustryFieldService.GetIndustryFieldByIdAsync(99));
        Assert.Equal("Industry field with ID 99 not found.", ex.Message);
    }
        
    [Fact]
    public async Task GetAllIndustryFieldsAsync_ReturnsList()
    {
        var list = new List<DimIndustryField>
        {
            new DimIndustryField(1, "A"),
            new DimIndustryField(2, "B")
        };
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.GetAllIndustryFieldsAsync())
            .ReturnsAsync(list);
        
        var result = (await _dimIndustryFieldService.GetAllIndustryFieldsAsync()).ToList();
        
        Assert.Equal(2, result.Count);
        Assert.Collection(result,
            item => Assert.Equal("A", item.IndustryFieldName),
            item => Assert.Equal("B", item.IndustryFieldName)
        );
    }

    [Fact]
    public async Task UpdateIndustryFieldAsync_WithValidParameters_ReturnsUpdated()
    {
        var existing = new DimIndustryField(7, "OldName");
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.GetIndustryFieldByIdAsync(7))
            .ReturnsAsync(existing);
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.UpdateIndustryFieldAsync(It.IsAny<DimIndustryField>()))
            .Returns(Task.CompletedTask);
        
        var updated = await _dimIndustryFieldService.UpdateIndustryFieldAsync(7, "NewName");
        
        Assert.Equal(7, updated.IndustryFieldId);
        Assert.Equal("NewName", updated.IndustryFieldName);
        _dimIndustryFieldRepositoryMock.Verify(r =>
            r.UpdateIndustryFieldAsync(
                It.Is<DimIndustryField>(f =>
                    f.IndustryFieldId == 7 &&
                    f.IndustryFieldName == "NewName"
                )), Times.Once);
    }
        
    [Fact]
    public async Task UpdateIndustryFieldAsync_NonExistingId_ThrowsNotFoundException()
    {
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.GetIndustryFieldByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new NotFoundException("Industry field with ID 123 not found."));
        
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimIndustryFieldService.UpdateIndustryFieldAsync(123, "Name"));
        Assert.Equal("Industry field with ID 123 not found.", ex.Message);
    }
        
    [Fact]
    public async Task UpdateIndustryFieldAsync_WithEmptyName_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimIndustryFieldService.UpdateIndustryFieldAsync(1, ""));
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
            .ThrowsAsync(new NotFoundException("Industry field with ID 88 not found."));
        
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimIndustryFieldService.DeleteIndustryFieldAsync(88));
        Assert.Equal("Industry field with ID 88 not found.", ex.Message);
    }
}