using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
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
    public async Task CreateDistrictAsync_EmptyRepo_CreatesWithId1()
    {
        _dimFederalDistrictRepositoryMock
            .Setup(r => r.AddFederalDistrictAsync(It.IsAny<DimFederalDistrict>()))
            .Callback<DimFederalDistrict>(d => d.DistrictId = 1)
            .Returns(Task.CompletedTask);
        
        var result = await _dimFederalDistrictService.CreateDistrictAsync("Central");
        
        Assert.Equal(1, result.DistrictId);
        Assert.Equal("Central", result.DistrictName);
        _dimFederalDistrictRepositoryMock.Verify(r =>
            r.AddFederalDistrictAsync(
                It.Is<DimFederalDistrict>(d =>
                    d.DistrictId   == 1 &&
                    d.DistrictName == "Central"
                )), Times.Once);
    }
    
    [Fact]
    public async Task CreateDistrictAsync_NonEmptyRepo_CreatesWithNextId()
    {
        _dimFederalDistrictRepositoryMock
            .Setup(r => r.AddFederalDistrictAsync(It.IsAny<DimFederalDistrict>()))
            .Callback<DimFederalDistrict>(d => d.DistrictId = 42)
            .Returns(Task.CompletedTask);
        
        var result = await _dimFederalDistrictService.CreateDistrictAsync("East");
        
        Assert.Equal(42, result.DistrictId);
        Assert.Equal("East", result.DistrictName);
        _dimFederalDistrictRepositoryMock.Verify(r =>
            r.AddFederalDistrictAsync(
                It.Is<DimFederalDistrict>(d =>
                    d.DistrictId   == 42 &&
                    d.DistrictName == "East"
                )), Times.Once);
    }
    
    [Fact]
    public async Task CreateDistrictAsync_RepositoryThrows_WrapsException()
    {
        _dimFederalDistrictRepositoryMock
            .Setup(r => r.AddFederalDistrictAsync(It.IsAny<DimFederalDistrict>()))
            .Callback<DimFederalDistrict>(d => d.DistrictId = 1)
            .ThrowsAsync(new Exception("db error"));
        
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimFederalDistrictService.CreateDistrictAsync("North")
        );
        Assert.Equal("An employer with ID 1 already exists.", ex.Message);
    }
    
    [Fact]
    public async Task GetDistrictByIdAsync_Existing_ReturnsDistrict()
    {
        var expected = new DimFederalDistrict(5, "South");
        _dimFederalDistrictRepositoryMock
            .Setup(r => r.GetFederalDistrictByIdAsync(5))
            .ReturnsAsync(expected);
        
        var actual = await _dimFederalDistrictService.GetDistrictByIdAsync(5);
        
        Assert.Same(expected, actual);
    }
    
    [Fact]
    public async Task GetDistrictByIdAsync_NotFound_ThrowsException()
    {
        _dimFederalDistrictRepositoryMock
            .Setup(r => r.GetFederalDistrictByIdAsync(7))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimFederalDistrictService.GetDistrictByIdAsync(7)
        );
        Assert.Equal("District with ID 7 was not found.", ex.Message);
    }
    
    [Fact]
    public async Task GetAllDistrictsAsync_ReturnsList()
    {
        var list = new List<DimFederalDistrict>
        {
            new DimFederalDistrict(1, "A"),
            new DimFederalDistrict(2, "B")
        };
        _dimFederalDistrictRepositoryMock
            .Setup(r => r.GetAllFederalDistrictsAsync())
            .ReturnsAsync(list);
        
        var result = (await _dimFederalDistrictService.GetAllDistrictsAsync()).ToList();
        
        Assert.Equal(list, result);
    }
    
    [Fact]
    public async Task UpdateDistrictAsync_Valid_UpdatesAndReturns()
    {
        var existing = new DimFederalDistrict(4, "OldName");
        _dimFederalDistrictRepositoryMock
            .Setup(r => r.GetFederalDistrictByIdAsync(4))
            .ReturnsAsync(existing);
        _dimFederalDistrictRepositoryMock
            .Setup(r => r.UpdateFederalDistrictAsync(It.IsAny<DimFederalDistrict>()))
            .Returns(Task.CompletedTask);
        
        var updated = await _dimFederalDistrictService.UpdateDistrictAsync(4, "NewName");
        
        Assert.Equal(4, updated.DistrictId);
        Assert.Equal("NewName", updated.DistrictName);
        _dimFederalDistrictRepositoryMock.Verify(r =>
            r.UpdateFederalDistrictAsync(
                It.Is<DimFederalDistrict>(d =>
                    d.DistrictId   == 4 &&
                    d.DistrictName == "NewName"
                )), Times.Once);
    }
    
    [Fact]
    public async Task UpdateDistrictAsync_NotFound_ThrowsException()
    {
        _dimFederalDistrictRepositoryMock
            .Setup(r => r.GetFederalDistrictByIdAsync(9))
            .ThrowsAsync(new KeyNotFoundException());
        
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimFederalDistrictService.UpdateDistrictAsync(9, "X")
        );
        Assert.Equal("Cannot update: district 9 was not found.", ex.Message);
    }
    
    [Fact]
    public async Task DeleteDistrictAsync_ValidId_CallsRepository()
    {
        _dimFederalDistrictRepositoryMock
            .Setup(r => r.DeleteFederalDistrictAsync(8))
            .Returns(Task.CompletedTask);
        
        await _dimFederalDistrictService.DeleteDistrictAsync(8);
        
        _dimFederalDistrictRepositoryMock.Verify(r =>
            r.DeleteFederalDistrictAsync(8), Times.Once);
    }
    
    [Fact]
    public async Task DeleteDistrictAsync_RepositoryThrows_WrapsException()
    {
        _dimFederalDistrictRepositoryMock
            .Setup(r => r.DeleteFederalDistrictAsync(10))
            .ThrowsAsync(new Exception("not found"));
        
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimFederalDistrictService.DeleteDistrictAsync(10)
        );
        Assert.Equal("Cannot delete: district 10 not found.", ex.Message);
    }
}