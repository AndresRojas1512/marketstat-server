using FluentAssertions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimCityService;
using MarketStat.Tests.Common.Builders;
using MarketStat.Tests.Common.Fixtures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services.UnitTests.Services.Dimensions;

[Trait("Category", "Unit")]
public class DimCityServiceUnitTests
{
    private readonly Mock<IDimCityRepository> _dimCityRepositoryMock;
    private readonly DimCityService _dimCityService;

    public DimCityServiceUnitTests()
    {
        _dimCityRepositoryMock = new Mock<IDimCityRepository>();
        var loggerMock = new Mock<ILogger<DimCityService>>();
        _dimCityService = new DimCityService(_dimCityRepositoryMock.Object, loggerMock.Object);
    }
    
    # region CreateCityAsync Tests
    
    [Fact]
    public async Task CreateCityAsync_ValidParameters_CallRepositoryAndReturnCity()
    {
        var newCityName = "Moscow";
        var newCityOblastId = 1;
        
        _dimCityRepositoryMock
            .Setup(r => r.AddCityAsync(It.IsAny<DimCity>()))
            .Callback<DimCity>(c => c.CityId = 1)
            .Returns(Task.CompletedTask);

        var result = await _dimCityService.CreateCityAsync(newCityName, newCityOblastId);

        result.Should().NotBeNull();
        result.CityId.Should().Be(1);
        result.CityName.Should().Be(newCityName);

        _dimCityRepositoryMock.Verify(r =>
            r.AddCityAsync(It.Is<DimCity>(c =>
                c.CityName == newCityName &&
                c.OblastId == newCityOblastId
            )), Times.Once);
    }
    
    [Fact]
    public async Task CreateCityAsync_RepositoryThrowsConflict_PropagateException()
    {
        var cityName = "Existing City";
        _dimCityRepositoryMock
            .Setup(r => r.AddCityAsync(It.IsAny<DimCity>()))
            .ThrowsAsync(new ConflictException("City already exists."));
        Func<Task> act = async () => await _dimCityService.CreateCityAsync(cityName, 1);
        await act.Should().ThrowAsync<ConflictException>().WithMessage("City already exists.");
    }
    
    # endregion
    
    # region GetCityByIdAsync Tests
    
    [Fact]
    public async Task GetCityByIdAsync_CityExists_ReturnCity()
    {
        var expectedCity = new DimCityBuilder().WithId(5).WithName("Tula").Build();
        _dimCityRepositoryMock
            .Setup(r => r.GetCityByIdAsync(5))
            .ReturnsAsync(expectedCity);
        var result = await _dimCityService.GetCityByIdAsync(5);
        result.Should().BeEquivalentTo(expectedCity);
    }
    
    [Fact]
    public async Task GetCityByIdAsync_NotFound_ThrowException()
    {
        _dimCityRepositoryMock
            .Setup(r => r.GetCityByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new NotFoundException("City not found."));
        Func<Task> act = async () => await _dimCityService.GetCityByIdAsync(999);
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("City not found.");
    }
    
    # endregion
    
    # region GetAllCitiesAsync Tests
    
    [Fact]
    public async Task GetAllCitiesAsync_ReturnAllCitiesList()
    {
        var cityList = new List<DimCity>
        {
            new DimCityBuilder().WithId(1).WithName("City A").Build(),
            new DimCityBuilder().WithId(2).WithName("City B").Build()
        };
        
        _dimCityRepositoryMock
            .Setup(r => r.GetAllCitiesAsync())
            .ReturnsAsync(cityList);

        var result = (await _dimCityService.GetAllCitiesAsync()).ToList();

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(cityList);
    }

    [Fact]
    public async Task GetAllCitiesAsync_NoCities_ReturnEmptyList()
    {
        _dimCityRepositoryMock
            .Setup(r => r.GetAllCitiesAsync())
            .ReturnsAsync(new List<DimCity>());
        var result = await _dimCityService.GetAllCitiesAsync();
        result.Should().BeEmpty();
    }
    
    # endregion
    
    # region UpdateCityAsync Tests
    
    [Fact]
    public async Task UpdateCityAsync_ValidData_ReturnUpdatedCity()
    {
        var existingCity = TestDataFactory.City();
        _dimCityRepositoryMock
            .Setup(r => r.GetCityByIdAsync(existingCity.CityId))
            .ReturnsAsync(existingCity);
        var result = await _dimCityService.UpdateCityAsync(existingCity.CityId, "New Name", 2);

        result.CityId.Should().Be(existingCity.CityId);
        result.CityName.Should().Be("New Name");
        result.OblastId.Should().Be(2);
        
        _dimCityRepositoryMock.Verify(r => r.UpdateCityAsync(It.Is<DimCity>(c => c.CityName == "New Name")), Times.Once);
    }
    
    [Fact]
    public async Task UpdateCityAsync_CityDoesNotExist_ThrowNotFoundException()
    {
        _dimCityRepositoryMock
            .Setup(r => r.GetCityByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new NotFoundException("Cannot update non-existing city."));
        Func<Task> act = async () => await _dimCityService.UpdateCityAsync(999, "New Name", 1);
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Cannot update non-existing city.");
    }
    
    # endregion
    
    # region DeleteCityAsync Tests
    
    [Fact]
    public async Task DeleteCityAsync_CityExists()
    {
        var cityIdToDelete = 7;
        await _dimCityService.DeleteCityAsync(7);
        _dimCityRepositoryMock.Verify(r => r.DeleteCityAsync(cityIdToDelete), Times.Once);
    }
    
    [Fact]
    public async Task DeleteCityAsync_CityDoesNotExist_ThrowNotFoundException()
    {
        var nonExistentId = 8;
        _dimCityRepositoryMock
            .Setup(r => r.DeleteCityAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException("Cannot delete non-existent city."));

        Func<Task> act = async () => await _dimCityService.DeleteCityAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Cannot delete non-existent city.");
    }
    
    # endregion
}