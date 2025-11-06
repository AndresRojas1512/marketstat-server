using FluentAssertions;
using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Repositories.PostgresRepositories.Dimensions;
using MarketStat.Tests.TestData.Builders.Dimensions;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Repository.Tests.Dimensions;

[Collection("Database collection")]
public class DimLocationRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public DimLocationRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    private DimLocationRepository CreateRepository(MarketStatDbContext context)
    {
        return new DimLocationRepository(context);
    }
    
    [Fact]
    public async Task AddLocationAsync_ShouldAddLocation_WhenDataIsCorrect()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var newLocation = new DimLocationBuilder()
            .WithId(0)
            .WithCityName("Moscow")
            .Build();
        await repository.AddLocationAsync(newLocation);
        var savedLocation = await context.DimLocations.FindAsync(newLocation.LocationId);
        savedLocation.Should().NotBeNull();
        savedLocation.CityName.Should().Be("Moscow");
        newLocation.LocationId.Should().Be(1);
    }
    
    [Fact]
    public async Task AddLocationAsync_ShouldThrowException_WhenLocationIsNull()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.AddLocationAsync(null!); 
        await act.Should().ThrowAsync<Exception>();
    }
    
    [Fact]
    public async Task GetLocationByIdAsync_ShouldReturnLocation_WhenLocationExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var expectedLocation = new DimLocationBuilder().WithId(1).Build();
        context.DimLocations.Add(DimLocationConverter.ToDbModel(expectedLocation));
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetLocationByIdAsync(1);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedLocation);
    }

    [Fact]
    public async Task GetLocationByIdAsync_ShouldThrowNotFoundException_WhenLocationDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.GetLocationByIdAsync(999);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    [Fact]
    public async Task GetAllLocationsAsync_ShouldReturnAllLocations_WhenLocationsExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var loc1 = DimLocationConverter.ToDbModel(new DimLocationBuilder().WithDistrictName("B District").Build());
        var loc2 = DimLocationConverter.ToDbModel(new DimLocationBuilder().WithDistrictName("A District").Build());
        context.DimLocations.AddRange(loc1, loc2);
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = (await repository.GetAllLocationsAsync()).ToList();
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().DistrictName.Should().Be("A District");
    }

    [Fact]
    public async Task GetAllLocationsAsync_ShouldReturnEmptyList_WhenNoLocationsExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var result = await repository.GetAllLocationsAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task UpdateLocationAsync_ShouldUpdateLocation_WhenLocationExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var originalDbModel = DimLocationConverter.ToDbModel(
            new DimLocationBuilder().WithId(1).WithCityName("Old City").Build()
        );
        context.DimLocations.Add(originalDbModel);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var repository = CreateRepository(context);
        var updatedLocation = new DimLocationBuilder()
            .WithId(1)
            .WithCityName("New City")
            .Build();
        await repository.UpdateLocationAsync(updatedLocation);
        var locInDb = await context.DimLocations.FindAsync(1);
        locInDb.Should().NotBeNull();
        locInDb.CityName.Should().Be("New City");
    }

    [Fact]
    public async Task UpdateLocationAsync_ShouldThrowNotFoundException_WhenLocationDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        var nonExistentLocation = new DimLocationBuilder().WithId(999).Build();
        Func<Task> act = async () => await repository.UpdateLocationAsync(nonExistentLocation);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task DeleteLocationAsync_ShouldDeleteLocation_WhenLocationExists()
    {
        await using var context = _fixture.CreateCleanContext();
        var locationId = 1;
        var dbModel = DimLocationConverter.ToDbModel(new DimLocationBuilder().WithId(locationId).Build());
        context.DimLocations.Add(dbModel);
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        (await context.DimLocations.FindAsync(locationId)).Should().NotBeNull();
        await repository.DeleteLocationAsync(locationId);
        (await context.DimLocations.FindAsync(locationId)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteLocationAsync_ShouldThrowNotFoundException_WhenLocationDoesNotExist()
    {
        await using var context = _fixture.CreateCleanContext();
        var repository = CreateRepository(context);
        Func<Task> act = async () => await repository.DeleteLocationAsync(999);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetLocationIdsByFilterAsync_ShouldReturnAllIds_WhenFiltersAreNull()
    {
        await using var context = _fixture.CreateCleanContext();
        context.DimLocations.AddRange(
            DimLocationConverter.ToDbModel(new DimLocationBuilder().WithId(1).Build()),
            DimLocationConverter.ToDbModel(new DimLocationBuilder().WithId(2).Build())
        );
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetLocationIdsByFilterAsync(null, null, null);
        result.Should().HaveCount(2);
        result.Should().Contain(new[] { 1, 2 });
    }
    
    [Fact]
    public async Task GetLocationIdsByFilterAsync_ShouldReturnFilteredIds_WhenOneFilterIsUsed()
    {
        await using var context = _fixture.CreateCleanContext();
        context.DimLocations.AddRange(
            DimLocationConverter.ToDbModel(new DimLocationBuilder().WithId(1).WithCityName("Moscow").Build()),
            DimLocationConverter.ToDbModel(new DimLocationBuilder().WithId(2).WithCityName("Tula").Build())
        );
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetLocationIdsByFilterAsync(null, null, "Tula");
        result.Should().HaveCount(1);
        result.Should().Contain(2);
    }
    
    [Fact]
    public async Task GetLocationIdsByFilterAsync_ShouldReturnFilteredIds_WhenAllFiltersAreUsed()
    {
        await using var context = _fixture.CreateCleanContext();
        context.DimLocations.AddRange(
            DimLocationConverter.ToDbModel(new DimLocationBuilder().WithId(1).WithDistrictName("D1").WithOblastName("O1").WithCityName("C1").Build()),
            DimLocationConverter.ToDbModel(new DimLocationBuilder().WithId(2).WithDistrictName("D1").WithOblastName("O1").WithCityName("C2").Build()),
            DimLocationConverter.ToDbModel(new DimLocationBuilder().WithId(3).WithDistrictName("D1").WithOblastName("O2").WithCityName("C1").Build())
        );
        await context.SaveChangesAsync();
        var repository = CreateRepository(context);
        var result = await repository.GetLocationIdsByFilterAsync("D1", "O1", "C2");
        result.Should().HaveCount(1);
        result.Should().Contain(2);
    }
}