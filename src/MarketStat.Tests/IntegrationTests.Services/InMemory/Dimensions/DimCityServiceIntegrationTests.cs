using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
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
    public async Task GetAllDates_Empty_ReturnsEmpty()
    {
        var all = await _dimCityService.GetAllCitiesAsync();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task GetAllDates_Seeded_ReturnsSeeded()
    {
        var seed = new List<DimCity>
        {
            new DimCity(1, "Moscow", 1),
            new DimCity(2, "Omsk", 2)
        };

        foreach (var d in seed)
            await _accessObject.DimCityRepository.AddCityAsync(d);

        var all = (await _dimCityService.GetAllCitiesAsync()).ToList();
        Assert.Contains(all, d => d.CityName == "Moscow");
        Assert.Contains(all, d => d.CityName == "Omsk");
    }
}