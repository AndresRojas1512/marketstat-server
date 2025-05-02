using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Services.Dimensions.DimOblastService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimOblastServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimOblastService _dimOblastService;
    
    public DimOblastServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimOblastService = new DimOblastService(_accessObject.DimOblastRepository, NullLogger<DimOblastService>.Instance);
    }
    public void Dispose() => _accessObject.Dispose();
    
    [Fact]
    public async Task GetAllOblasts_Empty_ReturnsEmpty()
    {
        var all = await _dimOblastService.GetAllOblastsAsync();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task GetAllIndustryFields_Seeded_ReturnsSeeded()
    {
        var seed = new List<DimOblast>
        {
            new DimOblast(1, "Tulskaya Oblast", 1),
            new DimOblast(2, "Leningradskaya Oblast", 2)
        };
        await _accessObject.SeedOblastAsync(seed);
        var all = (await _dimOblastService.GetAllOblastsAsync()).ToList();
        Assert.Contains(all, o => o.OblastName == "Tulskaya Oblast");
        Assert.Contains(all, o => o.OblastName == "Leningradskaya Oblast");
    }
}