using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Services.Dimensions.DimStandardJobRoleHierarchyService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimStandardJobRoleHierarchyIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimStandardJobRoleHierarchyService _dimStandardJobRoleHierarchyService;

    public DimStandardJobRoleHierarchyIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimStandardJobRoleHierarchyService = new DimStandardJobRoleHierarchyService(
            _accessObject.DimStandardJobRoleHierarchyRepository, NullLogger<DimStandardJobRoleHierarchyService>.Instance);
    }
    
    public void Dispose() => _accessObject.Dispose();

    [Fact]
    public async Task GetAllStandardJobRoleHierarchies_Empty_ReturnsEmpty()
    {
        var all = await _dimStandardJobRoleHierarchyService.GetAllStandardJobRoleHierarchiesAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task GetAllStandardJobRoleHierarchies_Seeded_ReturnsSeeded()
    {
        var seed = new List<DimStandardJobRoleHierarchy>
        {
            new DimStandardJobRoleHierarchy(1, 1),
            new DimStandardJobRoleHierarchy(2, 2)
        };
        await _accessObject.SeedStandardJobRoleHierarchyAsync(seed);
        var all = (await _dimStandardJobRoleHierarchyService.GetAllStandardJobRoleHierarchiesAsync()).ToList();
        
        Assert.Contains(all, jh =>
            jh.StandardJobRoleId == 1 &&
            jh.HierarchyLevelId == 1
        );

        Assert.Contains(all, jh =>
            jh.StandardJobRoleId == 2 &&
            jh.HierarchyLevelId == 2
        );
    }
}