using FluentAssertions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimLocationService;
using MarketStat.Tests.TestData.ObjectMothers.Dimensions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MarketStat.Services.Tests.Dimensions;

public class DimLocationServiceTests
{
    private readonly Mock<IDimLocationRepository> _mockRepository;
    private readonly Mock<ILogger<DimLocationService>> _mockLogger;
    private readonly DimLocationService _sut;

    public DimLocationServiceTests()
    {
        _mockRepository = new Mock<IDimLocationRepository>();
        _mockLogger = new Mock<ILogger<DimLocationService>>();
        
        _sut = new DimLocationService(_mockRepository.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task CreateLocationAsync_ShouldCallAddLocationAsync_WhenDataIsValid()
    {
        var newLocation = DimLocationObjectMother.ANewLocation();
        _mockRepository.Setup(repo => repo.AddLocationAsync(It.IsAny<DimLocation>()))
            .Callback<DimLocation>(loc => loc.LocationId = 1)
            .Returns(Task.CompletedTask);
        var result = await _sut.CreateLocationAsync(
            newLocation.CityName,
            newLocation.OblastName,
            newLocation.DistrictName
        );
        _mockRepository.Verify(repo => repo.AddLocationAsync(
            It.Is<DimLocation>(l => l.CityName == newLocation.CityName)
        ), Times.Once);
        result.Should().NotBeNull();
        result.LocationId.Should().Be(1);
        result.CityName.Should().Be(newLocation.CityName);
    }

    [Fact]
    public async Task CreateLocationAsync_ShouldThrowConflictException_WhenRepositoryThrowsConflict()
    {
        var newLocation = DimLocationObjectMother.ANewLocation();
        _mockRepository.Setup(repo => repo.AddLocationAsync(It.IsAny<DimLocation>()))
            .ThrowsAsync(new ConflictException("Location already exists."));
        Func<Task> act = async () => await _sut.CreateLocationAsync(
            newLocation.CityName,
            newLocation.OblastName,
            newLocation.DistrictName
        );
        await act.Should().ThrowAsync<ConflictException>();
    }
    
    [Fact]
    public async Task GetLocationByIdAsync_ShouldReturnLocation_WhenLocationExists()
    {
        var expectedLocation = DimLocationObjectMother.AnExistingLocation();
        _mockRepository.Setup(repo => repo.GetLocationByIdAsync(expectedLocation.LocationId))
            .ReturnsAsync(expectedLocation);
        var result = await _sut.GetLocationByIdAsync(expectedLocation.LocationId);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedLocation);
    }

    [Fact]
    public async Task GetLocationByIdAsync_ShouldThrowNotFoundException_WhenLocationDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.GetLocationByIdAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.GetLocationByIdAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetAllLocationsAsync_ShouldReturnAllLocations_WhenLocationsExist()
    {
        var expectedList = DimLocationObjectMother.SomeLocations();
        _mockRepository.Setup(repo => repo.GetAllLocationsAsync()).ReturnsAsync(expectedList);
        var result = await _sut.GetAllLocationsAsync();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedList);
    }

    [Fact]
    public async Task GetAllLocationsAsync_ShouldReturnEmptyList_WhenNoLocationsExist()
    {
        _mockRepository.Setup(repo => repo.GetAllLocationsAsync()).ReturnsAsync(new List<DimLocation>());
        var result = await _sut.GetAllLocationsAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task UpdateLocationAsync_ShouldCallUpdateLocationAsync_WhenDataIsValid()
    {
        var existingLocation = DimLocationObjectMother.AnExistingLocation();
        var updatedCity = "Tula";
        _mockRepository.Setup(repo => repo.GetLocationByIdAsync(existingLocation.LocationId))
                 .ReturnsAsync(existingLocation);
        
        _mockRepository.Setup(repo => repo.UpdateLocationAsync(It.IsAny<DimLocation>()))
                 .Returns(Task.CompletedTask);
        var result = await _sut.UpdateLocationAsync(
            existingLocation.LocationId,
            updatedCity,
            existingLocation.OblastName,
            existingLocation.DistrictName
        );
        _mockRepository.Verify(repo => repo.UpdateLocationAsync(
            It.Is<DimLocation>(l => 
                l.LocationId == existingLocation.LocationId &&
                l.CityName == updatedCity
            )
        ), Times.Once);
        result.CityName.Should().Be(updatedCity);
    }

    [Fact]
    public async Task UpdateLocationAsync_ShouldThrowNotFoundException_WhenLocationDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.GetLocationByIdAsync(nonExistentId))
                 .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.UpdateLocationAsync(nonExistentId, "Test", "Test", "Test");
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateLocationAsync_ShouldThrowConflictException_WhenRepositoryThrowsConflict()
    {
        var existingLocation = DimLocationObjectMother.AnExistingLocation();
        _mockRepository.Setup(repo => repo.GetLocationByIdAsync(existingLocation.LocationId))
                 .ReturnsAsync(existingLocation);
        _mockRepository.Setup(repo => repo.UpdateLocationAsync(It.IsAny<DimLocation>()))
                 .ThrowsAsync(new ConflictException("Conflict on update."));
        Func<Task> act = async () => await _sut.UpdateLocationAsync(
            existingLocation.LocationId, "Test", "Test", "Test"
        );
        await act.Should().ThrowAsync<ConflictException>();
    }
    
    [Fact]
    public async Task DeleteLocationAsync_ShouldCallDeleteLocationAsync_WhenLocationExists()
    {
        var existingId = 1;
        _mockRepository.Setup(repo => repo.DeleteLocationAsync(existingId))
            .Returns(Task.CompletedTask);
        await _sut.DeleteLocationAsync(existingId);
        _mockRepository.Verify(repo => repo.DeleteLocationAsync(existingId), Times.Once);
    }

    [Fact]
    public async Task DeleteLocationAsync_ShouldThrowNotFoundException_WhenLocationDoesNotExist()
    {
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.DeleteLocationAsync(nonExistentId))
            .ThrowsAsync(new NotFoundException("Not found."));
        Func<Task> act = async () => await _sut.DeleteLocationAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}