using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimFederalDistrictService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimFederalDistrictServiceUnitTests
{
    private readonly Mock<IDimFederalDistrictRepository> _dimFederalDistrictRepositoryMock;
    private readonly Mock<ILogger<DimFederalDistrictService>> _loggerMock;
    private readonly DimFederalDistrictService _dimFederalDistrictService;
    
    public DimFederalDistrictServiceUnitTests()
    {
        _dimFederalDistrictRepositoryMock = new Mock<IDimFederalDistrictRepository>();
        _loggerMock = new Mock<ILogger<DimFederalDistrictService>>();
        _dimFederalDistrictService =
            new DimFederalDistrictService(_dimFederalDistrictRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateDistrictAsync_ValidName_AssignsIdAndReturns()
    {
        _dimFederalDistrictRepositoryMock.Setup(r => r.AddFederalDistrictAsync(It.IsAny<DimFederalDistrict>()))
             .Callback<DimFederalDistrict>(d => d.DistrictId = 1)
             .Returns(Task.CompletedTask);

        var result = await _dimFederalDistrictService.CreateDistrictAsync("Central");

        Assert.Equal(1, result.DistrictId);
        Assert.Equal("Central", result.DistrictName);
        _dimFederalDistrictRepositoryMock.Verify(r => r.AddFederalDistrictAsync(
            It.Is<DimFederalDistrict>(d =>
                d.DistrictName == "Central")), Times.Once);
    }

    [Fact]
    public async Task CreateDistrictAsync_Duplicate_ThrowsConflictException()
    {
        _dimFederalDistrictRepositoryMock.Setup(r => r.AddFederalDistrictAsync(It.IsAny<DimFederalDistrict>()))
             .ThrowsAsync(new ConflictException("A federal district named 'West' already exists."));

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _dimFederalDistrictService.CreateDistrictAsync("West")
        );

        Assert.Equal("A federal district named 'West' already exists.", ex.Message);
    }

    [Fact]
    public async Task GetDistrictByIdAsync_Existing_ReturnsDomain()
    {
        var expected = new DimFederalDistrict(5, "North");
        _dimFederalDistrictRepositoryMock.Setup(r => r.GetFederalDistrictByIdAsync(5))
             .ReturnsAsync(expected);

        var actual = await _dimFederalDistrictService.GetDistrictByIdAsync(5);

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task GetDistrictByIdAsync_NotFound_ThrowsNotFoundException()
    {
        _dimFederalDistrictRepositoryMock.Setup(r => r.GetFederalDistrictByIdAsync(7))
             .ThrowsAsync(new NotFoundException("District 7 not found"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimFederalDistrictService.GetDistrictByIdAsync(7)
        );
    }

    [Fact]
    public async Task GetAllDistrictsAsync_ReturnsAll()
    {
        var list = new[]
        {
            new DimFederalDistrict(1, "A"),
            new DimFederalDistrict(2, "B")
        };
        _dimFederalDistrictRepositoryMock.Setup(r => r.GetAllFederalDistrictsAsync())
             .ReturnsAsync(list);

        var result = await _dimFederalDistrictService.GetAllDistrictsAsync();

        Assert.Equal(list, result);
    }

    [Fact]
    public async Task UpdateDistrictAsync_Valid_UpdatesAndReturns()
    {
        var existing = new DimFederalDistrict(4, "Old");
        _dimFederalDistrictRepositoryMock.Setup(r => r.GetFederalDistrictByIdAsync(4))
             .ReturnsAsync(existing);
        _dimFederalDistrictRepositoryMock.Setup(r => r.UpdateFederalDistrictAsync(It.IsAny<DimFederalDistrict>()))
             .Returns(Task.CompletedTask);

        var updated = await _dimFederalDistrictService.UpdateDistrictAsync(4, "New");

        Assert.Equal(4, updated.DistrictId);
        Assert.Equal("New", updated.DistrictName);
        _dimFederalDistrictRepositoryMock.Verify(r => r.UpdateFederalDistrictAsync(
            It.Is<DimFederalDistrict>(d =>
                d.DistrictId   == 4 &&
                d.DistrictName == "New")), Times.Once);
    }

    [Fact]
    public async Task UpdateDistrictAsync_NotFound_ThrowsNotFoundException()
    {
        _dimFederalDistrictRepositoryMock.Setup(r => r.GetFederalDistrictByIdAsync(9))
             .ThrowsAsync(new NotFoundException("nope"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimFederalDistrictService.UpdateDistrictAsync(9, "Any")
        );
    }

    [Fact]
    public async Task UpdateDistrictAsync_Conflict_ThrowsConflictException()
    {
        var existing = new DimFederalDistrict(8, "E");
        _dimFederalDistrictRepositoryMock.Setup(r => r.GetFederalDistrictByIdAsync(8))
             .ReturnsAsync(existing);
        _dimFederalDistrictRepositoryMock.Setup(r => r.UpdateFederalDistrictAsync(It.IsAny<DimFederalDistrict>()))
             .ThrowsAsync(new ConflictException("dup"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _dimFederalDistrictService.UpdateDistrictAsync(8, "Dup")
        );
    }

    [Fact]
    public async Task DeleteDistrictAsync_Valid_CallsRepository()
    {
        _dimFederalDistrictRepositoryMock.Setup(r => r.DeleteFederalDistrictAsync(10))
             .Returns(Task.CompletedTask);

        await _dimFederalDistrictService.DeleteDistrictAsync(10);

        _dimFederalDistrictRepositoryMock.Verify(r => r.DeleteFederalDistrictAsync(10), Times.Once);
    }

    [Fact]
    public async Task DeleteDistrictAsync_NotFound_ThrowsNotFoundException()
    {
        _dimFederalDistrictRepositoryMock.Setup(r => r.DeleteFederalDistrictAsync(11))
             .ThrowsAsync(new NotFoundException("gone"));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimFederalDistrictService.DeleteDistrictAsync(11)
        );
    }
}