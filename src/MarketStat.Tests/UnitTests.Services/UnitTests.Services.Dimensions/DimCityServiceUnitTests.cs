using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimCityService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

public class DimCityServiceUnitTests
{
    private readonly Mock<IDimCityRepository> _dimCityRepositoryMock;
    private readonly Mock<ILogger<DimCityService>> _loggerMock;
    private readonly DimCityService _dimCityService;

    public DimCityServiceUnitTests()
    {
        _dimCityRepositoryMock = new Mock<IDimCityRepository>();
        _loggerMock = new Mock<ILogger<DimCityService>>();
        _dimCityService = new DimCityService(_dimCityRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateCityAsync_ValidParameters_ReturnsNewCity()
    {
        // Arrange
        _dimCityRepositoryMock
            .Setup(r => r.GetAllCitiesAsync())
            .ReturnsAsync(new List<DimCity>());
        _dimCityRepositoryMock
            .Setup(r => r.AddCityAsync(It.IsAny<DimCity>()))
            .Returns(Task.CompletedTask);

        // Act
        var city = await _dimCityService.CreateCityAsync("Moscow", "Moscow Oblast", "Central");

        // Assert
        Assert.NotNull(city);
        Assert.Equal(1, city.CityId);
        Assert.Equal("Moscow", city.CityName);
        Assert.Equal("Moscow Oblast", city.OblastName);
        Assert.Equal("Central", city.FederalDistrict);

        _dimCityRepositoryMock.Verify(r => r.AddCityAsync(It.Is<DimCity>(c =>
            c.CityId == 1 &&
            c.CityName == "Moscow" &&
            c.OblastName == "Moscow Oblast" &&
            c.FederalDistrict == "Central"
        )), Times.Once);
    }
    
    [Fact]
    public async Task CreateCityAsync_Duplicate_ThrowsException()
    {
        // Arrange
        _dimCityRepositoryMock
            .Setup(r => r.GetAllCitiesAsync())
            .ReturnsAsync(new List<DimCity>());
        _dimCityRepositoryMock
            .Setup(r => r.AddCityAsync(It.IsAny<DimCity>()))
            .ThrowsAsync(new ArgumentException("duplicate"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimCityService.CreateCityAsync("Moscow", "Oblast", "District"));

        Assert.Equal("A city with ID 1 already exists.", ex.Message);
    }
    
    [Fact]
    public async Task CreateCityAsync_InvalidName_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimCityService.CreateCityAsync("", "Oblast", "District"));
    }
    
    [Fact]
    public async Task GetCityByIdAsync_Existing_ReturnsCity()
    {
        var expected = new DimCity(5, "Paris", "Ile-de-France", "Europe");
        _dimCityRepositoryMock
            .Setup(r => r.GetCityByIdAsync(5))
            .ReturnsAsync(expected);

        var actual = await _dimCityService.GetCityByIdAsync(5);

        Assert.Same(expected, actual);
    }
    
    [Fact]
    public async Task GetCityByIdAsync_NotFound_ThrowsException()
    {
        _dimCityRepositoryMock
            .Setup(r => r.GetCityByIdAsync(42))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimCityService.GetCityByIdAsync(42));

        Assert.Equal("City with ID 42 was not found.", ex.Message);
    }
    
    [Fact]
    public async Task GetAllCitiesAsync_ReturnsList()
    {
        var list = new List<DimCity>
        {
            new DimCity(1, "A", "OA", "FA"),
            new DimCity(2, "B", "OB", "FB")
        };
        _dimCityRepositoryMock
            .Setup(r => r.GetAllCitiesAsync())
            .ReturnsAsync(list);

        var result = (await _dimCityService.GetAllCitiesAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(list, result);
    }
    
    [Fact]
    public async Task UpdateCityAsync_ValidParameters_ReturnsUpdatedCity()
    {
        var existing = new DimCity(3, "Old", "OldOblast", "OldDistrict");
        _dimCityRepositoryMock
            .Setup(r => r.GetCityByIdAsync(3))
            .ReturnsAsync(existing);
        _dimCityRepositoryMock
            .Setup(r => r.UpdateCityAsync(existing))
            .Returns(Task.CompletedTask);

        var updated = await _dimCityService.UpdateCityAsync(3, "New", "NewOblast", "NewDistrict");

        Assert.Equal(3, updated.CityId);
        Assert.Equal("New", updated.CityName);
        Assert.Equal("NewOblast", updated.OblastName);
        Assert.Equal("NewDistrict", updated.FederalDistrict);

        _dimCityRepositoryMock.Verify(r => r.UpdateCityAsync(existing), Times.Once);
    }
    
    [Fact]
    public async Task UpdateCityAsync_NotFound_ThrowsException()
    {
        _dimCityRepositoryMock
            .Setup(r => r.GetCityByIdAsync(9))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimCityService.UpdateCityAsync(9, "X", "Y", "Z"));

        Assert.Equal("Cannot update: city 9 was not found.", ex.Message);
    }
    
    [Fact]
    public async Task UpdateCityAsync_InvalidName_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimCityService.UpdateCityAsync(1, null!, "Oblast", "District"));
    }
    
    [Fact]
    public async Task DeleteCityAsync_Existing_Completes()
    {
        _dimCityRepositoryMock
            .Setup(r => r.DeleteCityAsync(7))
            .Returns(Task.CompletedTask);

        await _dimCityService.DeleteCityAsync(7);

        _dimCityRepositoryMock.Verify(r => r.DeleteCityAsync(7), Times.Once);
    }
    
    [Fact]
    public async Task DeleteCityAsync_NotFound_ThrowsException()
    {
        _dimCityRepositoryMock
            .Setup(r => r.DeleteCityAsync(8))
            .ThrowsAsync(new KeyNotFoundException());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimCityService.DeleteCityAsync(8));

        Assert.Equal("Cannot delete: city 8 not found.", ex.Message);
    }
}