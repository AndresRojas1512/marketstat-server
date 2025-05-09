using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Services.Dimensions.DimCityService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimCityServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimCityService _dimCityService;

    public DimCityServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimCityService = new DimCityService(_accessObject.DimCityRepository, NullLogger<DimCityService>.Instance);
    }
    public void Dispose() => _accessObject.Dispose();
    
    [Fact]
    public async Task GetAllCities_Empty_ReturnsEmpty()
    {
        var all = await _dimCityService.GetAllCitiesAsync();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task CreateCity_PersistsAndGeneratesId()
    {
        await _accessObject.SeedFederalDistrictAsync(new[]
        {
            new DimFederalDistrict(1, "North District")
        });
        await _accessObject.SeedOblastAsync(new[]
        {
            new DimOblast(1, "Test Oblast", districtId: 1)
        });

        var created = await _dimCityService.CreateCityAsync("Springfield", oblastId: 1);
        Assert.True(created.CityId > 0);
        Assert.Equal("Springfield", created.CityName);
        Assert.Equal(1, created.OblastId);

        var fetched = await _dimCityService.GetCityByIdAsync(created.CityId);
        Assert.Equal(created.CityId, fetched.CityId);
        Assert.Equal("Springfield", fetched.CityName);
        Assert.Equal(1, fetched.OblastId);
    }
    
    [Fact]
    public async Task GetCityById_Nonexistent_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimCityService.GetCityByIdAsync(9999)
        );
    }
    
    [Fact]
    public async Task GetAllCities_Seeded_ReturnsSeeded()
    {
        await _accessObject.SeedFederalDistrictAsync(new[]
        {
            new DimFederalDistrict(1, "District A")
        });
        await _accessObject.SeedOblastAsync(new[]
        {
            new DimOblast(1, "Oblast A", districtId: 1)
        });
        await _accessObject.SeedCityAsync(new[]
        {
            new DimCity(1, "Alpha City", 1),
            new DimCity(2, "Beta Town",  1)
        });

        var all = (await _dimCityService.GetAllCitiesAsync()).ToList();
        Assert.Equal(2, all.Count);
        Assert.Contains(all, c => c.CityName == "Alpha City" && c.CityId == 1);
        Assert.Contains(all, c => c.CityName == "Beta Town"  && c.CityId == 2);
    }
    
    [Fact]
    public async Task UpdateCity_PersistsChanges()
    {
        await _accessObject.SeedFederalDistrictAsync(new[]
        {
            new DimFederalDistrict(1, "District X")
        });
        await _accessObject.SeedOblastAsync(new[]
        {
            new DimOblast(1, "Oblast X", districtId: 1)
        });

        var city = await _dimCityService.CreateCityAsync("OldName", oblastId: 1);
        var updated = await _dimCityService.UpdateCityAsync(
            city.CityId,
            "NewName",
            oblastId: 1
        );

        Assert.Equal(city.CityId, updated.CityId);
        Assert.Equal("NewName", updated.CityName);
        Assert.Equal(1, updated.OblastId);

        var fetched = await _dimCityService.GetCityByIdAsync(city.CityId);
        Assert.Equal("NewName", fetched.CityName);
    }
    
    [Fact]
    public async Task UpdateCity_InvalidId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimCityService.UpdateCityAsync(0, "Name", 1)
        );
    }
    
    [Fact]
    public async Task UpdateCity_NotFound_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimCityService.UpdateCityAsync(9999, "Name", 1)
        );
    }
    
    [Fact]
    public async Task DeleteCity_RemovesIt()
    {
        await _accessObject.SeedFederalDistrictAsync(new[]
        {
            new DimFederalDistrict(1, "District Y")
        });
        await _accessObject.SeedOblastAsync(new[]
        {
            new DimOblast(1, "Oblast Y", districtId: 1)
        });

        var city = await _dimCityService.CreateCityAsync("ToDelete", oblastId: 1);
        await _dimCityService.DeleteCityAsync(city.CityId);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimCityService.GetCityByIdAsync(city.CityId)
        );
    }

    [Fact]
    public async Task DeleteCity_NotFound_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimCityService.DeleteCityAsync(9999)
        );
    }
}