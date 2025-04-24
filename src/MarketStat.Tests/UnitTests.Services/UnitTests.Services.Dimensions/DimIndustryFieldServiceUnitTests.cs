using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
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
        // Arrange
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.GetAllIndustryFieldsAsync())
            .ReturnsAsync(new List<DimIndustryField>());
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.AddIndustryFieldAsync(It.IsAny<DimIndustryField>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _dimIndustryFieldService.CreateIndustryFieldAsync("Tech");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.IndustryFieldId);
        Assert.Equal("Tech", result.IndustryFieldName);
    }
    
    [Fact]
    public async Task CreateIndustryFieldAsync_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.GetAllIndustryFieldsAsync())
            .ReturnsAsync(new List<DimIndustryField>());
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.AddIndustryFieldAsync(It.IsAny<DimIndustryField>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimIndustryFieldService.CreateIndustryFieldAsync(""));
    }
    
    [Fact]
    public async Task GetIndustryFieldByIdAsync_ExistingId_ReturnsField()
    {
        // Arrange
        var existing = new DimIndustryField(42, "Finance");
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.GetIndustryFieldByIdAsync(42))
            .ReturnsAsync(existing);

        // Act
        var result = await _dimIndustryFieldService.GetIndustryFieldByIdAsync(42);

        // Assert
        Assert.Same(existing, result);
    }
    
    [Fact]
    public async Task GetIndustryFieldByIdAsync_NonExistingId_ThrowsException()
    {
        // Arrange
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.GetIndustryFieldByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimIndustryFieldService.GetIndustryFieldByIdAsync(99));
        Assert.Contains("99", ex.Message);
    }
    
    [Fact]
    public async Task GetAllIndustryFieldsAsync_ReturnsList()
    {
        // Arrange
        var list = new List<DimIndustryField>
        {
            new DimIndustryField(1, "A"),
            new DimIndustryField(2, "B")
        };
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.GetAllIndustryFieldsAsync())
            .ReturnsAsync(list);

        // Act
        var result = await _dimIndustryFieldService.GetAllIndustryFieldsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Collection(result,
            item => Assert.Equal("A", item.IndustryFieldName),
            item => Assert.Equal("B", item.IndustryFieldName)
        );
    }

    [Fact]
    public async Task UpdateIndustryFieldAsync_WithValidParameters_ReturnsUpdated()
    {
        // Arrange
        var existing = new DimIndustryField(7, "OldName");
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.GetIndustryFieldByIdAsync(7))
            .ReturnsAsync(existing);
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.UpdateIndustryFieldAsync(It.IsAny<DimIndustryField>()))
            .Returns(Task.CompletedTask);

        // Act
        var updated = await _dimIndustryFieldService.UpdateIndustryFieldAsync(7, "NewName");

        // Assert
        Assert.Equal(7, updated.IndustryFieldId);
        Assert.Equal("NewName", updated.IndustryFieldName);
    }
    
    [Fact]
    public async Task UpdateIndustryFieldAsync_NonExistingId_ThrowsException()
    {
        // Arrange
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.GetIndustryFieldByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimIndustryFieldService.UpdateIndustryFieldAsync(123, "Name"));
        Assert.Contains("123", ex.Message);
    }
    
    [Fact]
    public async Task UpdateIndustryFieldAsync_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimIndustryFieldService.UpdateIndustryFieldAsync(1, ""));
    }
    
    [Fact]
    public async Task DeleteIndustryFieldAsync_ExistingId_Completes()
    {
        // Arrange
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.DeleteIndustryFieldAsync(5))
            .Returns(Task.CompletedTask);

        // Act
        await _dimIndustryFieldService.DeleteIndustryFieldAsync(5);

        // Assert
        _dimIndustryFieldRepositoryMock.Verify(r => r.DeleteIndustryFieldAsync(5), Times.Once);
    }

    [Fact]
    public async Task DeleteIndustryFieldAsync_NonExistingId_ThrowsException()
    {
        // Arrange
        _dimIndustryFieldRepositoryMock
            .Setup(r => r.DeleteIndustryFieldAsync(It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimIndustryFieldService.DeleteIndustryFieldAsync(88));
        Assert.Contains("88", ex.Message);
    }
}