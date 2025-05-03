using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Services.Dimensions.DimDateService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimDateServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimDateService _dimDateService;

    public DimDateServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimDateService = new DimDateService(_accessObject.DimDateRepository, NullLogger<DimDateService>.Instance);
    }
    public void Dispose() => _accessObject.Dispose();
    
    [Fact]
    public async Task GetAllDates_Empty_ReturnsEmpty()
    {
        var all = await _dimDateService.GetAllDatesAsync();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task GetAllDates_Seeded_ReturnsSeeded()
    {
        var seed = new List<DimDate>
        {
            new DimDate(1, new DateOnly(2025, 1,  1), 2025, 1,  1),
            new DimDate(2, new DateOnly(2025, 4, 15), 2025, 2,  4)
        };

        await _accessObject.SeedDateAsync(seed);

        var all = (await _dimDateService.GetAllDatesAsync()).ToList();
        Assert.Contains(all, d => d.FullDate == new DateOnly(2025, 1,  1));
        Assert.Contains(all, d => d.FullDate == new DateOnly(2025, 4, 15));
    }
}