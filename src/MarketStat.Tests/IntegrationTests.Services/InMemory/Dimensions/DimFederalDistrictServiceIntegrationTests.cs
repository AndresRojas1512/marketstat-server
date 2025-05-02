using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Services.Dimensions.DimFederalDistrictService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimFederalDistrictServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimFederalDistrictService _dimFederalDistrictService;
    
    public DimFederalDistrictServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimFederalDistrictService = new DimFederalDistrictService(_accessObject.DimFederalDistrictRepository,
            NullLogger<DimFederalDistrictService>.Instance);
    }
    
    public void Dispose() => _accessObject.Dispose();
    
    [Fact]
    public async Task GetAllDistricts_Empty_ReturnsEmpty()
    {
        var all = await _dimFederalDistrictService.GetAllDistrictsAsync();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task GetAllIndustryFields_Seeded_ReturnsSeeded()
    {
        var seed = new List<DimFederalDistrict>
        {
            new DimFederalDistrict(1, "Central"),
            new DimFederalDistrict(2, "Volga")
        };
        await _accessObject.SeedFederalDistrictAsync(seed);
        var all = (await _dimFederalDistrictService.GetAllDistrictsAsync()).ToList();
        Assert.Contains(all, d => d.DistrictName == "Central");
        Assert.Contains(all, d => d.DistrictName == "Volga");
    }
}