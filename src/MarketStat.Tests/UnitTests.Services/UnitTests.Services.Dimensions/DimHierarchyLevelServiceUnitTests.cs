using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using Microsoft.Extensions.Logging;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimHierarchyLevelService;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimHierarchyLevelServiceUnitTests
{
    private readonly Mock<IDimHierarchyLevelRepository> _dimHierarchyLevelRepositoryMock;
    private readonly Mock<ILogger<DimHierarchyLevelService>> _loggerMock;
    private readonly DimHierarchyLevelService _dimHierarchyLevelService;

    public DimHierarchyLevelServiceUnitTests()
    {
        _dimHierarchyLevelRepositoryMock = new Mock<IDimHierarchyLevelRepository>();
        _loggerMock = new Mock<ILogger<DimHierarchyLevelService>>();
        _dimHierarchyLevelService = new DimHierarchyLevelService(_dimHierarchyLevelRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateHierarchyLevelAsync_EmptyRepo_CreatesWithId1()
    {
        _dimHierarchyLevelRepositoryMock
            .Setup(r => r.GetAllHierarchyLevelsAsync())
            .ReturnsAsync(new List<DimHierarchyLevel>());
        _dimHierarchyLevelRepositoryMock
            .Setup(r => r.AddHierarchyLevelAsync(It.IsAny<DimHierarchyLevel>()))
            .Returns(Task.CompletedTask);

        var result = await _dimHierarchyLevelService.CreateHierarchyLevelAsync("Junior");

        Assert.Equal(1, result.HierarchyLevelId);
        Assert.Equal("Junior", result.HierarchyLevelName);
        _dimHierarchyLevelRepositoryMock.Verify(r =>
            r.AddHierarchyLevelAsync(
                It.Is<DimHierarchyLevel>(h =>
                    h.HierarchyLevelId == 1 &&
                    h.HierarchyLevelName == "Junior"
                )), Times.Once);
    }
    
    [Fact]
    public async Task CreateHierarchyLevelAsync_NonEmptyRepo_CreatesWithNextId()
    {
        var existing = new List<DimHierarchyLevel>
        {
            new DimHierarchyLevel(5, "Mid")
        };
        _dimHierarchyLevelRepositoryMock
            .Setup(r => r.GetAllHierarchyLevelsAsync())
            .ReturnsAsync(existing);
        _dimHierarchyLevelRepositoryMock
            .Setup(r => r.AddHierarchyLevelAsync(It.IsAny<DimHierarchyLevel>()))
            .Returns(Task.CompletedTask);
        
        var result = await _dimHierarchyLevelService.CreateHierarchyLevelAsync("Senior");
        
        Assert.Equal(6, result.HierarchyLevelId);
        Assert.Equal("Senior", result.HierarchyLevelName);
        _dimHierarchyLevelRepositoryMock.Verify(r =>
            r.AddHierarchyLevelAsync(
                It.Is<DimHierarchyLevel>(h =>
                    h.HierarchyLevelId == 6 &&
                    h.HierarchyLevelName == "Senior"
                )), Times.Once);
    }
    
    [Fact]
    public async Task CreateHierarchyLevelAsync_RepositoryThrows_WrapsException()
    {
        _dimHierarchyLevelRepositoryMock
            .Setup(r => r.GetAllHierarchyLevelsAsync())
            .ReturnsAsync(new List<DimHierarchyLevel>());
        _dimHierarchyLevelRepositoryMock
            .Setup(r => r.AddHierarchyLevelAsync(It.IsAny<DimHierarchyLevel>()))
            .ThrowsAsync(new InvalidOperationException("db"));

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimHierarchyLevelService.CreateHierarchyLevelAsync("Lead"));
        Assert.Equal("Could not create HierarchyLevel Lead with id 1,", ex.Message);
    }
    
    [Fact]
    public async Task GetHierarchyLevelByIdAsync_Found_ReturnsInstance()
    {
        var existing = new DimHierarchyLevel(3, "Expert");
        _dimHierarchyLevelRepositoryMock
            .Setup(r => r.GetHierarchyLevelByIdAsync(3))
            .ReturnsAsync(existing);

        var result = await _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(3);

        Assert.Same(existing, result);
    }
    
    [Fact]
    public async Task GetHierarchyLevelByIdAsync_NotFound_ThrowsException()
    {
        _dimHierarchyLevelRepositoryMock
            .Setup(r => r.GetHierarchyLevelByIdAsync(7))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(7));
        Assert.Equal("Industry field 7 was not found.", ex.Message);
    }
    
    [Fact]
    public async Task GetAllHierarchyLevelsAsync_ReturnsList()
    {
        var list = new List<DimHierarchyLevel>
        {
            new DimHierarchyLevel(1, "A"),
            new DimHierarchyLevel(2, "B")
        };
        _dimHierarchyLevelRepositoryMock
            .Setup(r => r.GetAllHierarchyLevelsAsync())
            .ReturnsAsync(list);

        var result = (await _dimHierarchyLevelService.GetAllHierarchyLevelsAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(list, result);
    }
    
    [Fact]
    public async Task UpdateHierarchyLevelAsync_Valid_UpdatesAndReturns()
    {
        var existing = new DimHierarchyLevel(4, "OldName");
        _dimHierarchyLevelRepositoryMock
            .Setup(r => r.GetHierarchyLevelByIdAsync(4))
            .ReturnsAsync(existing);
        _dimHierarchyLevelRepositoryMock
            .Setup(r => r.UpdateHierarchyLevelAsync(It.IsAny<DimHierarchyLevel>()))
            .Returns(Task.CompletedTask);

        var result = await _dimHierarchyLevelService.UpdateHierarchyLevelAsync(4, "NewName");

        Assert.Equal(4, result.HierarchyLevelId);
        Assert.Equal("NewName", result.HierarchyLevelName);
        _dimHierarchyLevelRepositoryMock.Verify(r =>
            r.UpdateHierarchyLevelAsync(
                It.Is<DimHierarchyLevel>(h =>
                    h.HierarchyLevelId   == 4 &&
                    h.HierarchyLevelName == "NewName"
                )), Times.Once);
    }
    
    [Fact]
    public async Task UpdateHierarchyLevelAsync_NotFound_ThrowsException()
    {
        _dimHierarchyLevelRepositoryMock
            .Setup(r => r.GetHierarchyLevelByIdAsync(9))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimHierarchyLevelService.UpdateHierarchyLevelAsync(9, "X"));
        Assert.Equal("Cannot update: hierarchy level 9 not found.", ex.Message);
    }
    
    [Fact]
    public async Task DeleteHierarchyLevelAsync_Valid_CallsRepository()
    {
        _dimHierarchyLevelRepositoryMock
            .Setup(r => r.DeleteHierarchyLevelAsync(8))
            .Returns(Task.CompletedTask);

        await _dimHierarchyLevelService.DeleteHierarchyLevelAsync(8);

        _dimHierarchyLevelRepositoryMock.Verify(r =>
            r.DeleteHierarchyLevelAsync(8), Times.Once);
    }
    
    [Fact]
    public async Task DeleteHierarchyLevelAsync_RepositoryThrows_WrapsException()
    {
        _dimHierarchyLevelRepositoryMock
            .Setup(r => r.DeleteHierarchyLevelAsync(10))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimHierarchyLevelService.DeleteHierarchyLevelAsync(10));
        Assert.Equal("Cannot delete: hierarchy level 10 not found.", ex.Message);
    }
}