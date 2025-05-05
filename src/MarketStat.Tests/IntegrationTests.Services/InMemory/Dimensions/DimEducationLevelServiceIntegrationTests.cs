using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Services.Dimensions.DimEducationLevelService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimEducationLevelServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimEducationLevelService _dimEducationLevelService;

    public DimEducationLevelServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimEducationLevelService = new DimEducationLevelService(_accessObject.DimEducationLevelRepository,
            NullLogger<DimEducationLevelService>.Instance);
    }
    
    public void Dispose() => _accessObject.Dispose();

    [Fact]
    public async Task GetAllEducationLevels_Empty_ReturnsEmpty()
    {
        var all = await _dimEducationLevelService.GetAllEducationLevelsAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task GetAllEducationLevels_Seeded_ReturnsSeeded()
    {
        var seed = new List<DimEducationLevel>
        {
            new DimEducationLevel(1, "Bachelor"),
            new DimEducationLevel(2, "Master")
        };
        await _accessObject.SeedEducationLevelAsync(seed);

        var all = (await _dimEducationLevelService.GetAllEducationLevelsAsync()).ToList();
        Assert.Equal(2, all.Count);
        Assert.Contains(all, e => e.EducationLevelName == "Bachelor");
        Assert.Contains(all, e => e.EducationLevelName == "Master");
    }
    
    [Fact]
    public async Task CreateEducationLevelAsync_Valid_CreatesAndReturns()
    {
        var created = await _dimEducationLevelService.CreateEducationLevelAsync("PhD");

        Assert.True(created.EducationLevelId > 0);
        Assert.Equal("PhD", created.EducationLevelName);

        var fetched = await _dimEducationLevelService.GetEducationLevelByIdAsync(created.EducationLevelId);
        Assert.Equal("PhD", fetched.EducationLevelName);
    }
    
    [Fact]
    public async Task GetEducationLevelByIdAsync_NotFound_Throws()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _dimEducationLevelService.GetEducationLevelByIdAsync(999)
        );
    }
    
    [Fact]
    public async Task UpdateEducationLevelAsync_Valid_UpdatesAndReturns()
    {
        var seed = new DimEducationLevel(0, "Initial");
        await _accessObject.SeedEducationLevelAsync(new[] { seed });
        var existing = (await _dimEducationLevelService.GetAllEducationLevelsAsync()).Single();

        var updated = await _dimEducationLevelService.UpdateEducationLevelAsync(existing.EducationLevelId, "Updated");

        Assert.Equal(existing.EducationLevelId, updated.EducationLevelId);
        Assert.Equal("Updated", updated.EducationLevelName);

        var fetched = await _dimEducationLevelService.GetEducationLevelByIdAsync(existing.EducationLevelId);
        Assert.Equal("Updated", fetched.EducationLevelName);
    }
    
    [Fact]
    public async Task DeleteEducationLevelAsync_Valid_Removes()
    {
        var seed = new DimEducationLevel(0, "ToDelete");
        await _accessObject.SeedEducationLevelAsync(new[] { seed });
        var existing = (await _dimEducationLevelService.GetAllEducationLevelsAsync()).Single();

        await _dimEducationLevelService.DeleteEducationLevelAsync(existing.EducationLevelId);

        var remaining = await _dimEducationLevelService.GetAllEducationLevelsAsync();
        Assert.Empty(remaining);
    }
    
    [Fact]
    public async Task DeleteEducationLevelAsync_NotFound_Throws()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _dimEducationLevelService.DeleteEducationLevelAsync(12345)
        );
    }
}