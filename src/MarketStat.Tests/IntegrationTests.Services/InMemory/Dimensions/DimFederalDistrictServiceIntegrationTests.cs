using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
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
    public async Task CreateDistrictAsync_ValidName_CreatesAndReturnsDistrict()
    {
        var district = await _dimFederalDistrictService.CreateDistrictAsync("Central");
        Assert.True(district.DistrictId > 0);
        Assert.Equal("Central", district.DistrictName);

        var all = (await _dimFederalDistrictService.GetAllDistrictsAsync()).ToList();
        Assert.Single(all);
        Assert.Equal(district.DistrictId, all[0].DistrictId);
    }
    
    [Fact]
    public async Task GetDistrictByIdAsync_Existing_ReturnsDistrict()
    {
        var seed = new DimFederalDistrict(42, "Far East");
        await _accessObject.SeedFederalDistrictAsync(new[] { seed });

        var fetched = await _dimFederalDistrictService.GetDistrictByIdAsync(42);
        Assert.Equal(42, fetched.DistrictId);
        Assert.Equal("Far East", fetched.DistrictName);
    }
    
    [Fact]
    public async Task GetDistrictByIdAsync_NotFound_ThrowsException()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimFederalDistrictService.GetDistrictByIdAsync(999));
        Assert.Contains("not found", ex.Message);
    }
    
    [Fact]
    public async Task GetAllDistrictsAsync_Seeded_ReturnsAll()
    {
        var seeds = new[]
        {
            new DimFederalDistrict(1, "North"),
            new DimFederalDistrict(2, "South")
        };
        await _accessObject.SeedFederalDistrictAsync(seeds);

        var list = (await _dimFederalDistrictService.GetAllDistrictsAsync()).ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, d => d.DistrictId == 1 && d.DistrictName == "North");
        Assert.Contains(list, d => d.DistrictId == 2 && d.DistrictName == "South");
    }
    
    [Fact]
    public async Task UpdateDistrictAsync_Existing_UpdatesAndReturnsDistrict()
    {
        var original = new DimFederalDistrict(5, "OldName");
        await _accessObject.SeedFederalDistrictAsync(new[] { original });

        var updated = await _dimFederalDistrictService.UpdateDistrictAsync(5, "NewName");
        Assert.Equal(5, updated.DistrictId);
        Assert.Equal("NewName", updated.DistrictName);

        var fetched = await _dimFederalDistrictService.GetDistrictByIdAsync(5);
        Assert.Equal("NewName", fetched.DistrictName);
    }
    
    [Fact]
    public async Task UpdateDistrictAsync_NotFound_ThrowsException()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimFederalDistrictService.UpdateDistrictAsync(123, "X"));
        Assert.Contains("not found", ex.Message);
    }
    
    [Fact]
    public async Task DeleteDistrictAsync_Existing_RemovesDistrict()
    {
        var seed = new DimFederalDistrict(7, "ToRemove");
        await _accessObject.SeedFederalDistrictAsync(new[] { seed });

        await _dimFederalDistrictService.DeleteDistrictAsync(7);
        var all = (await _dimFederalDistrictService.GetAllDistrictsAsync()).ToList();
        Assert.DoesNotContain(all, d => d.DistrictId == 7);
    }
    
    [Fact]
    public async Task DeleteDistrictAsync_NotFound_ThrowsException()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimFederalDistrictService.DeleteDistrictAsync(888));
        Assert.Contains("not found", ex.Message);
    }
}