using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimHierarchyLevelService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimHierarchyLevelIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimHierarchyLevelService _dimHierarchyLevelService;
    
    private DimHierarchyLevel CreateTestHierarchyLevel(int id = 0, string code = "TEST", string name = "Test Level")
    {
        return new DimHierarchyLevel(id, code, name);
    }

    public DimHierarchyLevelIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimHierarchyLevelService = _accessObject.DimHierarchyLevelService;
    }

    public void Dispose() => _accessObject.Dispose();

    [Fact]
    public async Task CreateHierarchyLevelAsync_ValidParameters_CreatesAndReturnsNewLevel()
    {
        var code = "L1";
        var name = "Junior Specialist";
        
        var level = await _dimHierarchyLevelService.CreateHierarchyLevelAsync(code, name);
        
        Assert.True(level.HierarchyLevelId > 0, "HierarchyLevelId should be generated and > 0");
        Assert.Equal(code, level.HierarchyLevelCode);
        Assert.Equal(name, level.HierarchyLevelName);

        var all = (await _dimHierarchyLevelService.GetAllHierarchyLevelsAsync()).ToList();
        Assert.Single(all);
        Assert.Equal(level.HierarchyLevelId, all[0].HierarchyLevelId);
        Assert.Equal(code, all[0].HierarchyLevelCode);
    }

    [Fact]
    public async Task CreateHierarchyLevelAsync_InvalidParameters_ThrowsArgumentException()
    {
        
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimHierarchyLevelService.CreateHierarchyLevelAsync("", "Valid Name"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimHierarchyLevelService.CreateHierarchyLevelAsync("L1", ""));
    }

    [Fact]
    public async Task GetHierarchyLevelByIdAsync_Seeded_ReturnsLevel()
    {
        var seed = CreateTestHierarchyLevel(42, "L42", "Seeded Level");
        await _accessObject.SeedHierarchyLevelsAsync(new[] { seed });

        var fetched = await _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(42);

        Assert.NotNull(fetched);
        Assert.Equal(seed.HierarchyLevelId,   fetched.HierarchyLevelId);
        Assert.Equal(seed.HierarchyLevelCode, fetched.HierarchyLevelCode);
        Assert.Equal(seed.HierarchyLevelName, fetched.HierarchyLevelName);
    }

    [Fact]
    public async Task GetHierarchyLevelByIdAsync_NotFound_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(999));
    }

    [Fact]
    public async Task GetAllHierarchyLevelsAsync_Seeded_ReturnsAll()
    {
        var seeds = new[]
        {
            CreateTestHierarchyLevel(1, "L1", "One"),
            CreateTestHierarchyLevel(2, "L2", "Two")
        };
        await _accessObject.SeedHierarchyLevelsAsync(seeds);

        var list = (await _dimHierarchyLevelService.GetAllHierarchyLevelsAsync()).ToList();

        Assert.Equal(2, list.Count);
        Assert.Contains(list, h => h.HierarchyLevelId == 1 && h.HierarchyLevelName == "One");
        Assert.Contains(list, h => h.HierarchyLevelId == 2 && h.HierarchyLevelCode == "L2");
    }

    [Fact]
    public async Task UpdateHierarchyLevelAsync_Existing_UpdatesAndReturns()
    {
        var original = CreateTestHierarchyLevel(5, "L5-OLD", "Original Name");
        await _accessObject.SeedHierarchyLevelsAsync(new[] { original });

        var newCode = "L5-NEW";
        var newName = "Updated Name";
        
        var updated = await _dimHierarchyLevelService.UpdateHierarchyLevelAsync(5, newCode, newName);

        Assert.Equal(5, updated.HierarchyLevelId);
        Assert.Equal(newCode, updated.HierarchyLevelCode);
        Assert.Equal(newName, updated.HierarchyLevelName);
    
        var fetched = await _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(5);
        Assert.NotNull(fetched);
        Assert.Equal(newCode, fetched.HierarchyLevelCode);
        Assert.Equal(newName, fetched.HierarchyLevelName);
    }

    [Fact]
    public async Task UpdateHierarchyLevelAsync_NotFound_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimHierarchyLevelService.UpdateHierarchyLevelAsync(99, "L99", "Not Found Level"));
    }

    [Fact]
    public async Task UpdateHierarchyLevelAsync_InvalidParameters_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimHierarchyLevelService.UpdateHierarchyLevelAsync(0, "L0", "Name"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimHierarchyLevelService.UpdateHierarchyLevelAsync(1, "", "Name"));
    }

    [Fact]
    public async Task DeleteHierarchyLevelAsync_Existing_RemovesLevel()
    {
        var seed = CreateTestHierarchyLevel(7, "L7-DEL", "ToDelete");
        await _accessObject.SeedHierarchyLevelsAsync(new[] { seed });

        await _dimHierarchyLevelService.DeleteHierarchyLevelAsync(7);

        await Assert.ThrowsAsync<NotFoundException>(() => 
            _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(7)
        );
    }

    [Fact]
    public async Task DeleteHierarchyLevelAsync_NotFound_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimHierarchyLevelService.DeleteHierarchyLevelAsync(888)
        );
    }
}