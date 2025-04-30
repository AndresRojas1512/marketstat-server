using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimHierarchyLevelService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimHierarchyLevelIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimHierarchyLevelService _dimHierarchyLevelService;

    public DimHierarchyLevelIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimHierarchyLevelService = new DimHierarchyLevelService(_accessObject.DimHierarchyLevelRepository,
            NullLogger<DimHierarchyLevelService>.Instance);
    }
    public void Dispose() => _accessObject.Dispose();
    
    [Fact]
    public async Task GetAllHierarchyLevels_Empty_ReturnsEmpty()
    {
        var all = await _dimHierarchyLevelService.GetAllHierarchyLevelsAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task GetAllEmployers_Seeded_ReturnsSeeded()
    {
        var seed = new List<DimHierarchyLevel>
        {
            new DimHierarchyLevel(1, "Junior"),
            new DimHierarchyLevel(2, "Senior")
        };
        await _accessObject.SeedHierarchyLevelsAsync(seed);
        var all = (await _dimHierarchyLevelService.GetAllHierarchyLevelsAsync()).ToList();
        Assert.Contains(all, h => h.HierarchyLevelName == "Junior");
        Assert.Contains(all, h => h.HierarchyLevelName == "Senior");
    }
}