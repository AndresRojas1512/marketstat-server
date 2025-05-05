using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Services.Dimensions.DimIndustryFieldService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimIndustryFieldServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimIndustryFieldService _dimIndustryFieldService;

    public DimIndustryFieldServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimIndustryFieldService = new DimIndustryFieldService(_accessObject.IndustryFieldRepository,
            NullLogger<DimIndustryFieldService>.Instance);
    }
    public void Dispose() => _accessObject.Dispose();

    [Fact]
    public async Task CreateIndustryFieldAsync_ValidName_CreatesAndReturnsNewIndustryField()
    {
        var field = await _dimIndustryFieldService.CreateIndustryFieldAsync("Technology");

        Assert.True(field.IndustryFieldId > 0);
        Assert.Equal("Technology", field.IndustryFieldName);

        var all = (await _dimIndustryFieldService.GetAllIndustryFieldsAsync()).ToList();
        Assert.Single(all);
        Assert.Equal(field.IndustryFieldId, all[0].IndustryFieldId);
    }
    
    [Fact]
    public async Task GetAllIndustryFields_Seeded_ReturnsSeeded()
    {
        var seed = new List<DimIndustryField>
        {
            new DimIndustryField(1, "IT"),
            new DimIndustryField(2, "Finance")
        };
        await _accessObject.SeedIndustryFieldAsync(seed);
        var all = (await _dimIndustryFieldService.GetAllIndustryFieldsAsync()).ToList();
        Assert.Contains(all, i => i.IndustryFieldName == "IT");
        Assert.Contains(all, i => i.IndustryFieldName == "Finance");
    }
    
    [Fact]
    public async Task CreateIndustryFieldAsync_InvalidName_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimIndustryFieldService.CreateIndustryFieldAsync(""));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimIndustryFieldService.CreateIndustryFieldAsync(null!));
    }

    [Fact]
    public async Task GetIndustryFieldByIdAsync_Existing_ReturnsIndustryField()
    {
        var seed = new DimIndustryField(3, "Finance");
        await _accessObject.SeedIndustryFieldAsync(new[] { seed });

        var fetched = await _dimIndustryFieldService.GetIndustryFieldByIdAsync(3);

        Assert.Equal(seed.IndustryFieldId, fetched.IndustryFieldId);
        Assert.Equal(seed.IndustryFieldName, fetched.IndustryFieldName);
    }

    [Fact]
    public async Task GetIndustryFieldByIdAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _dimIndustryFieldService.GetIndustryFieldByIdAsync(999));
    }

    [Fact]
    public async Task GetAllIndustryFieldsAsync_Seeded_ReturnsAll()
    {
        var seeds = new[]
        {
            new DimIndustryField(1, "Aerospace"),
            new DimIndustryField(2, "Biotech")
        };
        await _accessObject.SeedIndustryFieldAsync(seeds);

        var list = (await _dimIndustryFieldService.GetAllIndustryFieldsAsync()).ToList();

        Assert.Equal(2, list.Count);
        Assert.Contains(list, f => f.IndustryFieldId == 1 && f.IndustryFieldName == "Aerospace");
        Assert.Contains(list, f => f.IndustryFieldId == 2 && f.IndustryFieldName == "Biotech");
    }

    [Fact]
    public async Task UpdateIndustryFieldAsync_Existing_UpdatesAndReturns()
    {
        var original = new DimIndustryField(5, "OldName");
        await _accessObject.SeedIndustryFieldAsync(new[] { original });

        var updated = await _dimIndustryFieldService.UpdateIndustryFieldAsync(5, "NewName");

        Assert.Equal(5, updated.IndustryFieldId);
        Assert.Equal("NewName", updated.IndustryFieldName);

        var fetched = await _dimIndustryFieldService.GetIndustryFieldByIdAsync(5);
        Assert.Equal("NewName", fetched.IndustryFieldName);
    }

    [Fact]
    public async Task UpdateIndustryFieldAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _dimIndustryFieldService.UpdateIndustryFieldAsync(123, "X"));
    }

    [Fact]
    public async Task UpdateIndustryFieldAsync_InvalidParameters_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimIndustryFieldService.UpdateIndustryFieldAsync(0, "X"));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimIndustryFieldService.UpdateIndustryFieldAsync(1, ""));
    }

    [Fact]
    public async Task DeleteIndustryFieldAsync_Existing_RemovesIndustryField()
    {
        var seed = new DimIndustryField(7, "ToRemove");
        await _accessObject.SeedIndustryFieldAsync(new[] { seed });

        await _dimIndustryFieldService.DeleteIndustryFieldAsync(7);

        var all = (await _dimIndustryFieldService.GetAllIndustryFieldsAsync()).ToList();
        Assert.Empty(all);
    }

    [Fact]
    public async Task DeleteIndustryFieldAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _dimIndustryFieldService.DeleteIndustryFieldAsync(888));
    }
}