using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
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
    public async Task CreateOblastAsync_ValidParameters_CreatesAndReturnsOblast()
    {
        var oblast = await _dimOblastService.CreateOblastAsync("Moscow Oblast", districtId: 1);
        Assert.True(oblast.OblastId > 0);
        Assert.Equal("Moscow Oblast", oblast.OblastName);
        Assert.Equal(1, oblast.DistrictId);
        
        var all = (await _dimOblastService.GetAllOblastsAsync()).ToList();
        Assert.Single(all);
        Assert.Equal(oblast.OblastId, all[0].OblastId);
    }

    [Fact]
    public async Task GetOblastByIdAsync_Existing_ReturnsOblast()
    {
        var seed = new DimOblast(42, "Tver Oblast", 2);
        await _accessObject.SeedOblastAsync(new[] { seed });
        
        var fetched = await _dimOblastService.GetOblastByIdAsync(42);
        Assert.Equal(42, fetched.OblastId);
        Assert.Equal("Tver Oblast", fetched.OblastName);
        Assert.Equal(2, fetched.DistrictId);
    }

    [Fact]
    public async Task GetOblastByIdAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimOblastService.GetOblastByIdAsync(999));
    }

    [Fact]
    public async Task GetAllOblastsAsync_Seeded_ReturnsAllOblasts()
    {
        var seeds = new[]
        {
            new DimOblast(1, "A", 1),
            new DimOblast(2, "B", 1),
            new DimOblast(3, "C", 2)
        };
        await _accessObject.SeedOblastAsync(seeds);
        
        var list = (await _dimOblastService.GetAllOblastsAsync()).ToList();
        Assert.Equal(3, list.Count);
        Assert.Contains(list, o => o.OblastName == "A");
        Assert.Contains(list, o => o.OblastName == "B");
        Assert.Contains(list, o => o.OblastName == "C");
    }

    [Fact]
    public async Task GetOblastsByFederalDistrictIdAsync_ReturnsOnlyMatching()
    {
        var seeds = new[]
        {
            new DimOblast(1, "X", 10),
            new DimOblast(2, "Y", 20),
            new DimOblast(3, "Z", 10),
        };
        await _accessObject.SeedOblastAsync(seeds);

        var for10 = (await _dimOblastService.GetOblastsByFederalDistrictIdAsync(10)).ToList();
        Assert.Equal(2, for10.Count);
        Assert.All(for10, o => Assert.Equal(10, o.DistrictId));

        var for30 = await _dimOblastService.GetOblastsByFederalDistrictIdAsync(30);
        Assert.Empty(for30);
    }

    [Fact]
    public async Task UpdateOblastAsync_Existing_UpdatesAndPersists()
    {
        var original = new DimOblast(5, "OldName", 3);
        await _accessObject.SeedOblastAsync(new[] { original });

        var updated = await _dimOblastService.UpdateOblastAsync(5, "NewName", 4);
        Assert.Equal(5, updated.OblastId);
        Assert.Equal("NewName", updated.OblastName);
        Assert.Equal(4, updated.DistrictId);

        var fetched = await _dimOblastService.GetOblastByIdAsync(5);
        Assert.Equal("NewName", fetched.OblastName);
        Assert.Equal(4, fetched.DistrictId);
    }

    [Fact]
    public async Task UpdateOblastAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimOblastService.UpdateOblastAsync(123, "Whatever", 1));
    }
        
    [Fact]
    public async Task DeleteOblastAsync_Existing_RemovesOblast()
    {
        var seed = new DimOblast(7, "ToDelete", 2);
        await _accessObject.SeedOblastAsync(new[] { seed });

        await _dimOblastService.DeleteOblastAsync(7);
        var all = (await _dimOblastService.GetAllOblastsAsync()).ToList();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task DeleteOblastAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimOblastService.DeleteOblastAsync(888));
    }
}