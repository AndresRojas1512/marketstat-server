using FluentAssertions;
using IntegrationTests.Services.AccessObject;
using IntegrationTests.Services.Fixtures;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimCityService;
using MarketStat.Tests.Common.Builders;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

[Trait("Category", "Integration")]
public class DimCityServiceIntegrationTests : IClassFixture<DimCityTestFixture>
{
    private readonly IDimCityService _dimCityService;
    private readonly MarketStatDbContext _dbContext;

    public DimCityServiceIntegrationTests(DimCityTestFixture fixture)
    {
        _dimCityService = fixture.DimCityService;
        _dbContext = fixture.DbContext;
    }
    
    # region CreateCityAsync Tests
    
    [Fact]
    public async Task CreateCityAsync_ValidData_PersistAndGenerateId()
    {
        var newCityName = "Moscow";
        var oblastId = 1;
        var created = await _dimCityService.CreateCityAsync(newCityName, oblastId);
        created.CityId.Should().BeGreaterThan(0);
        var fetched = await _dbContext.DimCities.FindAsync(created.CityId);
        fetched.Should().NotBeNull();
        fetched?.CityName.Should().Be(newCityName);
    }

    [Fact]
    public async Task CreateCityAsync_DuplicateNameInSameOblast_ThrowConflictException()
    {
        var city = new DimCityBuilder().WithName("Tula").WithOblastId(1).Build();
        _dbContext.DimCities.Add(new() { CityName = city.CityName, OblastId = city.OblastId });
        await _dbContext.SaveChangesAsync();

        Func<Task> act = async () => await _dimCityService.CreateCityAsync(city.CityName, city.OblastId);
        await act.Should().ThrowAsync<ConflictException>();
    }
    
    # endregion
    
    #region GetCityByIdAsync Tests
    
    [Fact]
    public async Task GetCityByIdAsync_CityExists()
    {
        var cityToFind = new DimCityBuilder().WithName("Sochi").WithOblastId(1).Build();
        _dbContext.DimCities.Add(new() { CityName = cityToFind.CityName, OblastId = cityToFind.OblastId });
        await _dbContext.SaveChangesAsync();
        
        var result = await _dimCityService.GetCityByIdAsync(cityToFind.CityId);
        result.Should().NotBeNull();
        result.CityName.Should().Be(cityToFind.CityName);
    }

    [Fact]
    public async Task GetCityByIdAsync_CityDoesNotExist_ThrowNotFoundException()
    {
        var nonExistentId = 9999;
        Func<Task> act = async () => await _dimCityService.GetCityByIdAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    #endregion
    
    #region GetAllCitiesAsync Tests
    
    [Fact]
    public async Task GetAllCitiesAsync_CitiesExist_ReturnsSeededCities()
    {
        var city1 = new DimCityBuilder().WithName("City A").WithOblastId(1).Build();
        var city2 = new DimCityBuilder().WithName("City B").WithOblastId(1).Build();
        _dbContext.DimCities.AddRange(new() { CityName = city1.CityName, OblastId = city1.OblastId }, 
            new() { CityName = city2.CityName, OblastId = city2.OblastId });
        await _dbContext.SaveChangesAsync();
        
        var result = (await _dimCityService.GetAllCitiesAsync()).ToList();
        
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.CityName == "City A");
        result.Should().Contain(c => c.CityName == "City B");
    }
    
    #endregion
    
    #region UpdateCityAsync Tests

    [Fact]
    public async Task UpdateCityAsync_CityExists_PersistChanges()
    {
        var cityToUpdate = new DimCityBuilder().WithName("OldName").WithOblastId(1).Build();
        _dbContext.DimCities.Add(new() { CityName = cityToUpdate.CityName, OblastId = cityToUpdate.OblastId });
        await _dbContext.SaveChangesAsync();

        var updated = await _dimCityService.UpdateCityAsync(cityToUpdate.CityId, "NewName", 1);
        updated.CityName.Should().Be("NewName");
        var fetched = await _dbContext.DimCities.FindAsync(updated.CityId);
        fetched?.CityName.Should().Be("NewName");
    }

    [Fact]
    public async Task UpdateCityAsync_CityDoesNotExist_ThrowNotFoundException()
    {
        var nonExistentId = 9999;
        Func<Task> act = async () => await _dimCityService.UpdateCityAsync(nonExistentId, "Name", 1);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    #endregion
    
    #region DeleteCityAsync Tests

    [Fact]
    public async Task DeleteCityAsync_CityExists()
    {
        var cityToDelete = new DimCityBuilder().WithName("ToDelete").WithOblastId(1).Build();
        _dbContext.DimCities.Add(new() { CityName = cityToDelete.CityName, OblastId = cityToDelete.OblastId });
        await _dbContext.SaveChangesAsync();

        await _dimCityService.DeleteCityAsync(cityToDelete.CityId);
        var fetched = await _dbContext.DimCities.FindAsync(cityToDelete.CityId);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCityAsync_CityDoesNotExist_ThrowNotFoundException()
    {
        var nonExistentId = 9999;
        Func<Task> act = async () => await _dimCityService.DeleteCityAsync(nonExistentId);
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    #endregion
}