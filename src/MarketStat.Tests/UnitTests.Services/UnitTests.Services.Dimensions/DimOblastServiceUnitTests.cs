using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimOblastService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimOblastServiceUnitTests
{
    private readonly Mock<IDimOblastRepository> _dimOblastRepositoryMock;
    private readonly Mock<ILogger<DimOblastService>> _loggerMock;
    private readonly DimOblastService _dimOblastService;

    public DimOblastServiceUnitTests()
    {
        _dimOblastRepositoryMock = new Mock<IDimOblastRepository>();
        _loggerMock = new Mock<ILogger<DimOblastService>>();
        _dimOblastService = new DimOblastService(_dimOblastRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateOblastAsync_EmptyRepo_CreatesWithId1()
    {
        _dimOblastRepositoryMock
            .Setup(r => r.GetAllOblastsAsync())
            .ReturnsAsync(new List<DimOblast>());
        _dimOblastRepositoryMock
            .Setup(r => r.AddOblastAsync(It.IsAny<DimOblast>()))
            .Returns(Task.CompletedTask);

        var result = await _dimOblastService.CreateOblastAsync("TestRegion", 2);

        Assert.Equal(1, result.OblastId);
        Assert.Equal("TestRegion", result.OblastName);
        Assert.Equal(2, result.DistrictId);
        _dimOblastRepositoryMock.Verify(r =>
            r.AddOblastAsync(
                It.Is<DimOblast>(o =>
                    o.OblastId   == 1 &&
                    o.OblastName == "TestRegion" &&
                    o.DistrictId == 2
                )), Times.Once);
    }
    
    [Fact]
    public async Task CreateOblastAsync_NonEmptyRepo_CreatesWithNextId()
    {
        var existing = new List<DimOblast> { new DimOblast(5, "Old", 1) };
        _dimOblastRepositoryMock
            .Setup(r => r.GetAllOblastsAsync())
            .ReturnsAsync(existing);
        _dimOblastRepositoryMock
            .Setup(r => r.AddOblastAsync(It.IsAny<DimOblast>()))
            .Returns(Task.CompletedTask);
        
        var result = await _dimOblastService.CreateOblastAsync("NewRegion", 3);
        
        Assert.Equal(6, result.OblastId);
        Assert.Equal("NewRegion", result.OblastName);
        Assert.Equal(3, result.DistrictId);
        _dimOblastRepositoryMock.Verify(r =>
            r.AddOblastAsync(
                It.Is<DimOblast>(o =>
                    o.OblastId   == 6 &&
                    o.OblastName == "NewRegion" &&
                    o.DistrictId == 3
                )), Times.Once);
    }
    
    [Fact]
    public async Task CreateOblastAsync_RepositoryThrows_WrapsException()
    {
        _dimOblastRepositoryMock
            .Setup(r => r.GetAllOblastsAsync())
            .ReturnsAsync(new List<DimOblast>());
        _dimOblastRepositoryMock
            .Setup(r => r.AddOblastAsync(It.IsAny<DimOblast>()))
            .ThrowsAsync(new Exception("db fail"));
        
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimOblastService.CreateOblastAsync("XRegion", 4)
        );
        Assert.Equal("Could not create oblast XRegion", ex.Message);
    }
    
    [Fact]
    public async Task GetOblastByIdAsync_Existing_ReturnsOblast()
    {
        var expected = new DimOblast(3, "Foo", 2);
        _dimOblastRepositoryMock
            .Setup(r => r.GetOblastByIdAsync(3))
            .ReturnsAsync(expected);

        var actual = await _dimOblastService.GetOblastByIdAsync(3);

        Assert.Same(expected, actual);
    }
    
    [Fact]
    public async Task GetOblastByIdAsync_NotFound_ThrowsException()
    {
        _dimOblastRepositoryMock
            .Setup(r => r.GetOblastByIdAsync(7))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimOblastService.GetOblastByIdAsync(7)
        );
        Assert.Equal("Oblast 7 was not found.", ex.Message);
    }
    
    [Fact]
    public async Task GetAllOblastsAsync_ReturnsList()
    {
        var list = new List<DimOblast>
        {
            new DimOblast(1, "A", 1),
            new DimOblast(2, "B", 2)
        };
        _dimOblastRepositoryMock
            .Setup(r => r.GetAllOblastsAsync())
            .ReturnsAsync(list);

        var result = (await _dimOblastService.GetAllOblastsAsync()).ToList();

        Assert.Equal(list, result);
    }
    
    [Fact]
    public async Task GetOblastsByFederalDistrictIdAsync_ValidId_ReturnsList()
    {
        const int districtId = 5;
        var list = new List<DimOblast>
        {
            new DimOblast(1, "X", districtId),
            new DimOblast(2, "Y", districtId)
        };
        _dimOblastRepositoryMock
            .Setup(r => r.GetOblastsByFederalDistrictIdAsync(districtId))
            .ReturnsAsync(list);

        var result = (await _dimOblastService.GetOblastsByFederalDistrictIdAsync(districtId)).ToList();

        Assert.Equal(list, result);
    }
    
    [Fact]
    public async Task GetOblastsByFederalDistrictIdAsync_RepositoryThrows_WrapsException()
    {
        _dimOblastRepositoryMock
            .Setup(r => r.GetOblastsByFederalDistrictIdAsync(3))
            .ThrowsAsync(new Exception("db error"));

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimOblastService.GetOblastsByFederalDistrictIdAsync(3)
        );
        Assert.Equal("Could not retrieve oblasts for district 3.", ex.Message);
    }
    
    [Fact]
    public async Task UpdateOblastAsync_ValidParameters_UpdatesAndReturns()
    {
        var existing = new DimOblast(4, "OldName", 1);
        _dimOblastRepositoryMock
            .Setup(r => r.GetOblastByIdAsync(4))
            .ReturnsAsync(existing);
        _dimOblastRepositoryMock
            .Setup(r => r.UpdateOblastAsync(It.IsAny<DimOblast>()))
            .Returns(Task.CompletedTask);

        var updated = await _dimOblastService.UpdateOblastAsync(4, "NewName", 2);

        Assert.Equal(4, updated.OblastId);
        Assert.Equal("NewName", updated.OblastName);
        Assert.Equal(2, updated.DistrictId);
        _dimOblastRepositoryMock.Verify(r =>
            r.UpdateOblastAsync(
                It.Is<DimOblast>(o =>
                    o.OblastId   == 4 &&
                    o.OblastName == "NewName" &&
                    o.DistrictId == 2
                )), Times.Once);
    }
    
    [Fact]
    public async Task UpdateOblastAsync_NotFound_ThrowsException()
    {
        _dimOblastRepositoryMock
            .Setup(r => r.GetOblastByIdAsync(9))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimOblastService.UpdateOblastAsync(9, "X", 2)
        );
        Assert.Equal("Cannot update: oblast 9 not found.", ex.Message);
    }
    
    [Fact]
    public async Task DeleteOblastAsync_ValidId_CallsRepository()
    {
        _dimOblastRepositoryMock
            .Setup(r => r.DeleteOblastAsync(8))
            .Returns(Task.CompletedTask);

        await _dimOblastService.DeleteOblastAsync(8);

        _dimOblastRepositoryMock.Verify(r =>
            r.DeleteOblastAsync(8), Times.Once);
    }
    
    [Fact]
    public async Task DeleteOblastAsync_RepositoryThrows_WrapsException()
    {
        _dimOblastRepositoryMock
            .Setup(r => r.DeleteOblastAsync(10))
            .ThrowsAsync(new Exception("not found"));

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimOblastService.DeleteOblastAsync(10)
        );
        Assert.Equal("Cannot delete: oblast 10 not found.", ex.Message);
    }
}