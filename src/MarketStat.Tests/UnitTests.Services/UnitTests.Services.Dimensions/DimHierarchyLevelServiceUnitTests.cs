using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
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
    public async Task CreateHierarchyLevelAsync_ValidParameters_ReturnsNewLevel()
    {
        _dimHierarchyLevelRepositoryMock.Setup(r => r.AddHierarchyLevelAsync(It.IsAny<DimHierarchyLevel>()))
             .Callback<DimHierarchyLevel>(h => h.HierarchyLevelId = 1)
             .Returns(Task.CompletedTask);

        var result = await _dimHierarchyLevelService.CreateHierarchyLevelAsync("Junior");

        Assert.Equal(1, result.HierarchyLevelId);
        Assert.Equal("Junior", result.HierarchyLevelName);
        _dimHierarchyLevelRepositoryMock.Verify(r => r.AddHierarchyLevelAsync(
            It.Is<DimHierarchyLevel>(h =>
                h.HierarchyLevelName == "Junior")), Times.Once);
    }
    
    [Fact]
    public async Task CreateHierarchyLevelAsync_Duplicate_ThrowsConflictException()
    {
        _dimHierarchyLevelRepositoryMock.Setup(r => r.AddHierarchyLevelAsync(It.IsAny<DimHierarchyLevel>()))
             .ThrowsAsync(new ConflictException("dup"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _dimHierarchyLevelService.CreateHierarchyLevelAsync("Senior")
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreateHierarchyLevelAsync_InvalidName_ThrowsArgumentException(string name)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimHierarchyLevelService.CreateHierarchyLevelAsync(name!)
        );
    }

    [Fact]
    public async Task GetHierarchyLevelByIdAsync_Existing_ReturnsLevel()
    {
        var expected = new DimHierarchyLevel(5, "Expert");
        _dimHierarchyLevelRepositoryMock.Setup(r => r.GetHierarchyLevelByIdAsync(5))
             .ReturnsAsync(expected);

        var actual = await _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(5);

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task GetHierarchyLevelByIdAsync_NotFound_ThrowsNotFoundException()
    {
        _dimHierarchyLevelRepositoryMock.Setup(r => r.GetHierarchyLevelByIdAsync(7))
             .ThrowsAsync(new NotFoundException("nf"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(7)
        );
    }

    [Fact]
    public async Task GetAllHierarchyLevelsAsync_ReturnsList()
    {
        var list = new[]
        {
            new DimHierarchyLevel(1, "A"),
            new DimHierarchyLevel(2, "B")
        };
        _dimHierarchyLevelRepositoryMock.Setup(r => r.GetAllHierarchyLevelsAsync())
             .ReturnsAsync(list);

        var result = await _dimHierarchyLevelService.GetAllHierarchyLevelsAsync();

        Assert.Equal(list, result);
    }

    [Fact]
    public async Task UpdateHierarchyLevelAsync_ValidParameters_UpdatesAndReturns()
    {
        var existing = new DimHierarchyLevel(4, "OldName");
        _dimHierarchyLevelRepositoryMock.Setup(r => r.GetHierarchyLevelByIdAsync(4))
             .ReturnsAsync(existing);
        _dimHierarchyLevelRepositoryMock.Setup(r => r.UpdateHierarchyLevelAsync(It.IsAny<DimHierarchyLevel>()))
             .Returns(Task.CompletedTask);

        var updated = await _dimHierarchyLevelService.UpdateHierarchyLevelAsync(4, "NewName");

        Assert.Equal(4, updated.HierarchyLevelId);
        Assert.Equal("NewName", updated.HierarchyLevelName);
        _dimHierarchyLevelRepositoryMock.Verify(r => r.UpdateHierarchyLevelAsync(
            It.Is<DimHierarchyLevel>(h =>
                h.HierarchyLevelId   == 4 &&
                h.HierarchyLevelName == "NewName")), Times.Once);
    }

    [Theory]
    [InlineData(0, "X")]
    [InlineData(3, null)]
    [InlineData(3, "")]
    public async Task UpdateHierarchyLevelAsync_InvalidParameters_ThrowsArgumentException(int id, string name)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimHierarchyLevelService.UpdateHierarchyLevelAsync(id, name!)
        );
    }

    [Fact]
    public async Task UpdateHierarchyLevelAsync_NotFound_ThrowsNotFoundException()
    {
        _dimHierarchyLevelRepositoryMock.Setup(r => r.GetHierarchyLevelByIdAsync(9))
             .ThrowsAsync(new NotFoundException("nf"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimHierarchyLevelService.UpdateHierarchyLevelAsync(9, "Any")
        );
    }

    [Fact]
    public async Task UpdateHierarchyLevelAsync_Conflict_ThrowsConflictException()
    {
        var existing = new DimHierarchyLevel(8, "E");
        _dimHierarchyLevelRepositoryMock.Setup(r => r.GetHierarchyLevelByIdAsync(8))
             .ReturnsAsync(existing);
        _dimHierarchyLevelRepositoryMock.Setup(r => r.UpdateHierarchyLevelAsync(It.IsAny<DimHierarchyLevel>()))
             .ThrowsAsync(new ConflictException("dup"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _dimHierarchyLevelService.UpdateHierarchyLevelAsync(8, "Dup")
        );
    }

    [Fact]
    public async Task DeleteHierarchyLevelAsync_Valid_CallsRepository()
    {
        _dimHierarchyLevelRepositoryMock.Setup(r => r.DeleteHierarchyLevelAsync(8))
             .Returns(Task.CompletedTask);

        await _dimHierarchyLevelService.DeleteHierarchyLevelAsync(8);

        _dimHierarchyLevelRepositoryMock.Verify(r => r.DeleteHierarchyLevelAsync(8), Times.Once);
    }

    [Fact]
    public async Task DeleteHierarchyLevelAsync_NotFound_ThrowsNotFoundException()
    {
        _dimHierarchyLevelRepositoryMock.Setup(r => r.DeleteHierarchyLevelAsync(10))
             .ThrowsAsync(new NotFoundException("nf"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimHierarchyLevelService.DeleteHierarchyLevelAsync(10)
        );
    }
}