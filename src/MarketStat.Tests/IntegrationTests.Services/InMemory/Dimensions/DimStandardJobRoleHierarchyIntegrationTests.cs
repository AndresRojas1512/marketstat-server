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
    public async Task CreateStandardJobRoleHierarchy_Valid_CreatesAndReturnsLink()
    {
        var link = await _dimStandardJobRoleHierarchyService.CreateStandardJobRoleHierarchy(1, 2);
        Assert.Equal(1, link.StandardJobRoleId);
        Assert.Equal(2, link.HierarchyLevelId);

        var all = (await _dimStandardJobRoleHierarchyService.GetAllStandardJobRoleHierarchiesAsync()).ToList();
        Assert.Single(all);
        Assert.Equal(1, all[0].StandardJobRoleId);
        Assert.Equal(2, all[0].HierarchyLevelId);
    }

    [Fact]
    public async Task CreateStandardJobRoleHierarchy_Duplicate_ThrowsException()
    {
        await _dimStandardJobRoleHierarchyService.CreateStandardJobRoleHierarchy(1, 2);
        await Assert.ThrowsAsync<Exception>(() =>
            _dimStandardJobRoleHierarchyService.CreateStandardJobRoleHierarchy(1, 2));
    }

    [Fact]
    public async Task GetStandardJobRoleHierarchyAsync_Existing_ReturnsLink()
    {
        var seed = new DimStandardJobRoleHierarchy(3, 4);
        await _accessObject.SeedStandardJobRoleHierarchyAsync(new[] { seed });

        var fetched = await _dimStandardJobRoleHierarchyService.GetStandardJobRoleHierarchyAsync(3, 4);
        Assert.Equal(3, fetched.StandardJobRoleId);
        Assert.Equal(4, fetched.HierarchyLevelId);
    }

    [Fact]
    public async Task GetStandardJobRoleHierarchyAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _dimStandardJobRoleHierarchyService.GetStandardJobRoleHierarchyAsync(99, 88));
    }

    [Fact]
    public async Task GetLevelsByJobRoleIdAsync_Seeded_ReturnsLevels()
    {
        var seeds = new[]
        {
            new DimStandardJobRoleHierarchy(5, 10),
            new DimStandardJobRoleHierarchy(5, 11),
            new DimStandardJobRoleHierarchy(6, 10)
        };
        await _accessObject.SeedStandardJobRoleHierarchyAsync(seeds);

        var levels = (await _dimStandardJobRoleHierarchyService.GetLevelsByJobRoleIdAsync(5)).ToList();
        Assert.Equal(2, levels.Count);
        Assert.Contains(levels, l => l.HierarchyLevelId == 10);
        Assert.Contains(levels, l => l.HierarchyLevelId == 11);
    }

    [Fact]
    public async Task GetJobRolesByLevelIdAsync_Seeded_ReturnsJobRoles()
    {
        var seeds = new[]
        {
            new DimStandardJobRoleHierarchy(7, 20),
            new DimStandardJobRoleHierarchy(8, 20),
            new DimStandardJobRoleHierarchy(7, 21)
        };
        await _accessObject.SeedStandardJobRoleHierarchyAsync(seeds);

        var jobs = (await _dimStandardJobRoleHierarchyService.GetJobRolesByLevelIdAsync(20)).ToList();
        Assert.Equal(2, jobs.Count);
        Assert.Contains(jobs, j => j.StandardJobRoleId == 7);
        Assert.Contains(jobs, j => j.StandardJobRoleId == 8);
    }

    [Fact]
    public async Task GetAllStandardJobRoleHierarchiesAsync_Seeded_ReturnsAll()
    {
        var seeds = new[]
        {
            new DimStandardJobRoleHierarchy(9, 30),
            new DimStandardJobRoleHierarchy(10, 31)
        };
        await _accessObject.SeedStandardJobRoleHierarchyAsync(seeds);

        var all = (await _dimStandardJobRoleHierarchyService.GetAllStandardJobRoleHierarchiesAsync()).ToList();
        Assert.Equal(2, all.Count);
        Assert.Contains(all, x => x.StandardJobRoleId == 9 && x.HierarchyLevelId == 30);
        Assert.Contains(all, x => x.StandardJobRoleId == 10 && x.HierarchyLevelId == 31);
    }

    [Fact]
    public async Task DeleteStandardJobRoleHierarchyAsync_Existing_RemovesLink()
    {
        var seed = new DimStandardJobRoleHierarchy(11, 40);
        await _accessObject.SeedStandardJobRoleHierarchyAsync(new[] { seed });

        await _dimStandardJobRoleHierarchyService.DeleteStandardJobRoleHierarchyAsync(11, 40);
        var all = (await _dimStandardJobRoleHierarchyService.GetAllStandardJobRoleHierarchiesAsync()).ToList();
        Assert.Empty(all);
    }

    [Fact]
    public async Task DeleteStandardJobRoleHierarchyAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _dimStandardJobRoleHierarchyService.DeleteStandardJobRoleHierarchyAsync(99, 88));
    }
}