using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Services.Dimensions.DimIndustryFieldService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimIndustryFieldServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimIndustryFieldService _dimIndustryFieldService;

    private DimIndustryField CreateTestIndustryField(int id = 0, string code = "TEST", string name = "Test Industry")
    {
        return new DimIndustryField(id, code, name);
    }

    public DimIndustryFieldServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimIndustryFieldService = _accessObject.IndustryFieldService;
    }

    public void Dispose() => _accessObject.Dispose();

    [Fact]
    public async Task CreateIndustryFieldAsync_ValidParameters_CreatesAndReturnsNewIndustryField()
    {
        var code = "TECH";
        var name = "Technology";

        var field = await _dimIndustryFieldService.CreateIndustryFieldAsync(code, name);
        
        Assert.True(field.IndustryFieldId > 0);
        Assert.Equal(code, field.IndustryFieldCode);
        Assert.Equal(name, field.IndustryFieldName);

        var all = (await _dimIndustryFieldService.GetAllIndustryFieldsAsync()).ToList();
        Assert.Single(all);
        Assert.Equal(field.IndustryFieldId, all[0].IndustryFieldId);
        Assert.Equal(code, all[0].IndustryFieldCode);
    }

    [Fact]
    public async Task CreateIndustryFieldAsync_InvalidName_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimIndustryFieldService.CreateIndustryFieldAsync("CODE", ""));
            
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimIndustryFieldService.CreateIndustryFieldAsync("", "Valid Name"));
    }

    [Fact]
    public async Task GetIndustryFieldByIdAsync_Existing_ReturnsIndustryField()
    {
        var seed = CreateTestIndustryField(42, "FIN", "Finance");
        await _accessObject.SeedIndustryFieldAsync(new[] { seed });

        var fetched = await _dimIndustryFieldService.GetIndustryFieldByIdAsync(42);

        Assert.NotNull(fetched);
        Assert.Equal(seed.IndustryFieldId, fetched.IndustryFieldId);
        Assert.Equal(seed.IndustryFieldCode, fetched.IndustryFieldCode);
        Assert.Equal(seed.IndustryFieldName, fetched.IndustryFieldName);
    }

    [Fact]
    public async Task GetIndustryFieldByIdAsync_NotFound_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimIndustryFieldService.GetIndustryFieldByIdAsync(999));
    }

    [Fact]
    public async Task GetAllIndustryFieldsAsync_Seeded_ReturnsAll()
    {
        var seeds = new[]
        {
            CreateTestIndustryField(1, "AERO", "Aerospace"),
            CreateTestIndustryField(2, "BIO", "Biotech")
        };
        await _accessObject.SeedIndustryFieldAsync(seeds);

        var list = (await _dimIndustryFieldService.GetAllIndustryFieldsAsync()).ToList();

        Assert.Equal(2, list.Count);
        Assert.Contains(list, f => f.IndustryFieldId == 1 && f.IndustryFieldCode == "AERO");
        Assert.Contains(list, f => f.IndustryFieldId == 2 && f.IndustryFieldName == "Biotech");
    }

    [Fact]
    public async Task UpdateIndustryFieldAsync_Existing_UpdatesAndReturns()
    {
        var original = CreateTestIndustryField(5, "OLD-CODE", "OldName");
        await _accessObject.SeedIndustryFieldAsync(new[] { original });

        var newCode = "NEW-CODE";
        var newName = "NewName";
        
        var updated = await _dimIndustryFieldService.UpdateIndustryFieldAsync(5, newCode, newName);

        Assert.Equal(5, updated.IndustryFieldId);
        Assert.Equal(newCode, updated.IndustryFieldCode);
        Assert.Equal(newName, updated.IndustryFieldName);

        var fetched = await _dimIndustryFieldService.GetIndustryFieldByIdAsync(5);
        Assert.NotNull(fetched);
        Assert.Equal(newCode, fetched.IndustryFieldCode);
        Assert.Equal(newName, fetched.IndustryFieldName);
    }

    [Fact]
    public async Task UpdateIndustryFieldAsync_NotFound_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimIndustryFieldService.UpdateIndustryFieldAsync(123, "CODE", "Name"));
    }

    [Fact]
    public async Task UpdateIndustryFieldAsync_InvalidParameters_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimIndustryFieldService.UpdateIndustryFieldAsync(0, "CODE", "Name"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimIndustryFieldService.UpdateIndustryFieldAsync(1, "", "Name"));
    }

    [Fact]
    public async Task DeleteIndustryFieldAsync_Existing_RemovesIndustryField()
    {
        var seed = CreateTestIndustryField(7, "DEL", "ToRemove");
        await _accessObject.SeedIndustryFieldAsync(new[] { seed });

        await _dimIndustryFieldService.DeleteIndustryFieldAsync(7);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimIndustryFieldService.GetIndustryFieldByIdAsync(7)
        );
    }

    [Fact]
    public async Task DeleteIndustryFieldAsync_NotFound_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimIndustryFieldService.DeleteIndustryFieldAsync(888));
    }
}