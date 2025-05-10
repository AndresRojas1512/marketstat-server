using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
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
    public async Task CreateOblastAsync_ValidParameters_AssignsIdAndReturns()
    {
        _dimOblastRepositoryMock
            .Setup(r => r.AddOblastAsync(It.IsAny<DimOblast>()))
            .Callback<DimOblast>(o => o.OblastId = 1)
            .Returns(Task.CompletedTask);

        var result = await _dimOblastService.CreateOblastAsync("TestRegion", districtId: 2);

        Assert.Equal(1, result.OblastId);
        Assert.Equal("TestRegion", result.OblastName);
        Assert.Equal(2, result.DistrictId);

        _dimOblastRepositoryMock.Verify(r => r.AddOblastAsync(
            It.Is<DimOblast>(o =>
                o.OblastId   == 1 &&
                o.OblastName == "TestRegion" &&
                o.DistrictId == 2
            )), Times.Once);
    }

    [Theory]
    [InlineData("", 1)]
    [InlineData("Name", 0)]
    public async Task CreateOblastAsync_InvalidParameters_ThrowsArgumentException(string name, int districtId)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimOblastService.CreateOblastAsync(name, districtId));
    }

    [Fact]
    public async Task CreateOblastAsync_RepositoryThrowsConflict_PropagatesConflictException()
    {
        _dimOblastRepositoryMock
            .Setup(r => r.AddOblastAsync(It.IsAny<DimOblast>()))
            .ThrowsAsync(new ConflictException("duplicate"));

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _dimOblastService.CreateOblastAsync("XRegion", 4));
        Assert.Equal("duplicate", ex.Message);
    }

    [Fact]
    public async Task CreateOblastAsync_RepositoryThrowsNotFound_PropagatesNotFoundException()
    {
        _dimOblastRepositoryMock
            .Setup(r => r.AddOblastAsync(It.IsAny<DimOblast>()))
            .ThrowsAsync(new NotFoundException("FK not found"));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimOblastService.CreateOblastAsync("XRegion", 4));
        Assert.Equal("FK not found", ex.Message);
    }

    [Fact]
    public async Task CreateOblastAsync_RepositoryThrowsGeneral_PropagatesOriginalException()
    {
        _dimOblastRepositoryMock
            .Setup(r => r.AddOblastAsync(It.IsAny<DimOblast>()))
            .ThrowsAsync(new InvalidOperationException("db fail"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _dimOblastService.CreateOblastAsync("XRegion", 4));
        Assert.Equal("db fail", ex.Message);
    }

    [Fact]
    public async Task GetOblastByIdAsync_ExistingId_ReturnsOblast()
    {
        var expected = new DimOblast(3, "Foo", 2);
        _dimOblastRepositoryMock.Setup(r => r.GetOblastByIdAsync(3)).ReturnsAsync(expected);

        var actual = await _dimOblastService.GetOblastByIdAsync(3);

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task GetOblastByIdAsync_NotFound_ThrowsNotFoundException()
    {
        _dimOblastRepositoryMock
            .Setup(r => r.GetOblastByIdAsync(7))
            .ThrowsAsync(new NotFoundException("Oblast 7 not found"));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimOblastService.GetOblastByIdAsync(7));
        Assert.Equal("Oblast 7 not found", ex.Message);
    }

    [Fact]
    public async Task GetAllOblastsAsync_ReturnsList()
    {
        var list = new List<DimOblast>
        {
            new DimOblast(1, "A", 1),
            new DimOblast(2, "B", 2)
        };
        _dimOblastRepositoryMock.Setup(r => r.GetAllOblastsAsync()).ReturnsAsync(list);

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
        _dimOblastRepositoryMock.Setup(r => r.GetOblastsByFederalDistrictIdAsync(districtId)).ReturnsAsync(list);

        var result = (await _dimOblastService.GetOblastsByFederalDistrictIdAsync(districtId)).ToList();

        Assert.Equal(list, result);
    }

    [Fact]
    public async Task GetOblastsByFederalDistrictIdAsync_RepositoryThrowsGeneral_PropagatesOriginalException()
    {
        _dimOblastRepositoryMock.Setup(r => r.GetOblastsByFederalDistrictIdAsync(3))
             .ThrowsAsync(new InvalidOperationException("db error"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _dimOblastService.GetOblastsByFederalDistrictIdAsync(3));
        Assert.Equal("db error", ex.Message);
    }

    [Fact]
    public async Task UpdateOblastAsync_ValidParameters_UpdatesAndReturns()
    {
        var existing = new DimOblast(4, "OldName", 1);
        _dimOblastRepositoryMock.Setup(r => r.GetOblastByIdAsync(4)).ReturnsAsync(existing);
        _dimOblastRepositoryMock.Setup(r => r.UpdateOblastAsync(It.IsAny<DimOblast>())).Returns(Task.CompletedTask);

        var updated = await _dimOblastService.UpdateOblastAsync(4, "NewName", 2);

        Assert.Equal(4, updated.OblastId);
        Assert.Equal("NewName", updated.OblastName);
        Assert.Equal(2, updated.DistrictId);

        _dimOblastRepositoryMock.Verify(r => r.UpdateOblastAsync(
            It.Is<DimOblast>(o =>
                o.OblastId   == 4 &&
                o.OblastName == "NewName" &&
                o.DistrictId == 2
            )), Times.Once);
    }

    [Fact]
    public async Task UpdateOblastAsync_NotFound_ThrowsNotFoundException()
    {
        _dimOblastRepositoryMock.Setup(r => r.GetOblastByIdAsync(9))
             .ThrowsAsync(new NotFoundException("Oblast 9 not found"));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimOblastService.UpdateOblastAsync(9, "X", 2));
        Assert.Equal("Oblast 9 not found", ex.Message);
    }

    [Fact]
    public async Task UpdateOblastAsync_Conflict_ThrowsConflictException()
    {
        var existing = new DimOblast(5, "Old", 1);
        _dimOblastRepositoryMock.Setup(r => r.GetOblastByIdAsync(5)).ReturnsAsync(existing);
        _dimOblastRepositoryMock.Setup(r => r.UpdateOblastAsync(It.IsAny<DimOblast>()))
             .ThrowsAsync(new ConflictException("duplicate"));

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _dimOblastService.UpdateOblastAsync(5, "New", 1));
        Assert.Equal("duplicate", ex.Message);
    }

    [Fact]
    public async Task DeleteOblastAsync_ValidId_CallsRepository()
    {
        _dimOblastRepositoryMock.Setup(r => r.DeleteOblastAsync(8)).Returns(Task.CompletedTask);

        await _dimOblastService.DeleteOblastAsync(8);

        _dimOblastRepositoryMock.Verify(r => r.DeleteOblastAsync(8), Times.Once);
    }

    [Fact]
    public async Task DeleteOblastAsync_NotFound_ThrowsNotFoundException()
    {
        _dimOblastRepositoryMock.Setup(r => r.DeleteOblastAsync(10))
             .ThrowsAsync(new NotFoundException("Oblast 10 not found"));

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimOblastService.DeleteOblastAsync(10));
        Assert.Equal("Oblast 10 not found", ex.Message);
    }
}