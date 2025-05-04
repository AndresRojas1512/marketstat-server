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
    public async Task CreateHierarchyLevelAsync_ValidName_CreatesAndReturnsNewLevel()
    {
        var name = "Level A";
        var level = await _dimHierarchyLevelService.CreateHierarchyLevelAsync(name);

        Assert.True(level.HierarchyLevelId > 0);
        Assert.Equal(name, level.HierarchyLevelName);

        var all = (await _dimHierarchyLevelService.GetAllHierarchyLevelsAsync()).ToList();
        Assert.Single(all);
        Assert.Equal(level.HierarchyLevelId, all[0].HierarchyLevelId);
    }

    [Fact]
    public async Task CreateHierarchyLevelAsync_InvalidName_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimHierarchyLevelService.CreateHierarchyLevelAsync(null!));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimHierarchyLevelService.CreateHierarchyLevelAsync(string.Empty));
    }
    
    [Fact]
    public async Task GetHierarchyLevelByIdAsync_Seeded_ReturnsLevel()
    {
        var seed = new DimHierarchyLevel(42, "Seeded");
        await _accessObject.SeedHierarchyLevelsAsync(new[] { seed });

        var fetched = await _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(42);
        Assert.Equal(seed.HierarchyLevelId,   fetched.HierarchyLevelId);
        Assert.Equal(seed.HierarchyLevelName, fetched.HierarchyLevelName);
    }
    
    [Fact]
    public async Task GetHierarchyLevelByIdAsync_NotFound_ThrowsException()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(99));

        Assert.Equal("Industry field 99 was not found.", ex.Message);
    }
    
    [Fact]
    public async Task GetAllHierarchyLevelsAsync_Seeded_ReturnsAll()
    {
        var seeds = new[]
        {
            new DimHierarchyLevel(1, "One"),
            new DimHierarchyLevel(2, "Two")
        };
        await _accessObject.SeedHierarchyLevelsAsync(seeds);

        var list = (await _dimHierarchyLevelService.GetAllHierarchyLevelsAsync()).ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, h => h.HierarchyLevelId == 1 && h.HierarchyLevelName == "One");
        Assert.Contains(list, h => h.HierarchyLevelId == 2 && h.HierarchyLevelName == "Two");
    }
    
    [Fact]
    public async Task UpdateHierarchyLevelAsync_Existing_UpdatesAndReturns()
    {
        var original = new DimHierarchyLevel(5, "Original");
        await _accessObject.SeedHierarchyLevelsAsync(new[] { original });

        var updated = await _dimHierarchyLevelService.UpdateHierarchyLevelAsync(5, "Updated");
        Assert.Equal(5, updated.HierarchyLevelId);
        Assert.Equal("Updated", updated.HierarchyLevelName);

        var fetched = await _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(5);
        Assert.Equal("Updated", fetched.HierarchyLevelName);
    }
    
    [Fact]
    public async Task UpdateHierarchyLevelAsync_NotFound_ThrowsException()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimHierarchyLevelService.UpdateHierarchyLevelAsync(99, "X"));

        Assert.Equal("Cannot update: hierarchy level 99 not found.", ex.Message);
    }

    [Fact]
    public async Task UpdateHierarchyLevelAsync_InvalidParameters_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimHierarchyLevelService.UpdateHierarchyLevelAsync(0, "Name"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimHierarchyLevelService.UpdateHierarchyLevelAsync(1, ""));
    }

    [Fact]
    public async Task DeleteHierarchyLevelAsync_Existing_RemovesLevel()
    {
        var seed = new DimHierarchyLevel(7, "ToDelete");
        await _accessObject.SeedHierarchyLevelsAsync(new[] { seed });

        await _dimHierarchyLevelService.DeleteHierarchyLevelAsync(7);

        var all = (await _dimHierarchyLevelService.GetAllHierarchyLevelsAsync()).ToList();
        Assert.Empty(all);
    }

    [Fact]
    public async Task DeleteHierarchyLevelAsync_NotFound_ThrowsException()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _dimHierarchyLevelService.DeleteHierarchyLevelAsync(123));

        Assert.Equal("Cannot delete: hierarchy level 123 not found.", ex.Message);
    }
}