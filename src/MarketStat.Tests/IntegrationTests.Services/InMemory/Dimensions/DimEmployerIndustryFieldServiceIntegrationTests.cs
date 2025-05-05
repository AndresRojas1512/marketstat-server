using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerIndustryFieldService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimEmployerIndustryFieldServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimEmployerIndustryFieldService _dimEmployerIndustryFieldService;

    public DimEmployerIndustryFieldServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimEmployerIndustryFieldService = new DimEmployerIndustryFieldService(
            _accessObject.DimEmployerIndustryFieldRepository, NullLogger<DimEmployerIndustryFieldService>.Instance);
    }
    public void Dispose() => _accessObject.Dispose();
    
    [Fact]
    public async Task GetAllEmployerIndustryFieldsAsync_Empty_ReturnsEmpty()
    {
        var all = await _dimEmployerIndustryFieldService.GetAllEmployerIndustryFieldsAsync();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task CreateEmployerIndustryFieldAsync_ValidIds_CreatesAndReturnsLink()
    {
        var link = await _dimEmployerIndustryFieldService.CreateEmployerIndustryFieldAsync(10, 20);
        Assert.Equal(10, link.EmployerId);
        Assert.Equal(20, link.IndustryFieldId);

        var all = (await _dimEmployerIndustryFieldService.GetAllEmployerIndustryFieldsAsync()).ToList();
        Assert.Single(all);
        Assert.Equal((10,20), (all[0].EmployerId, all[0].IndustryFieldId));
    }
    
    [Fact]
    public async Task CreateEmployerIndustryFieldAsync_InvalidParameters_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployerIndustryFieldService.CreateEmployerIndustryFieldAsync(0, 5));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployerIndustryFieldService.CreateEmployerIndustryFieldAsync(5, 0));
    }
    
    [Fact]
    public async Task CreateEmployerIndustryFieldAsync_Duplicate_ThrowsException()
    {
        var seed = new DimEmployerIndustryField(1, 2);
        await _accessObject.SeedEmployerIndustryFieldAsync(new[] { seed });

        await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployerIndustryFieldService.CreateEmployerIndustryFieldAsync(1, 2));
    }
    
    [Fact]
    public async Task GetEmployerIndustryFieldAsync_Existing_ReturnsLink()
    {
        var seed = new DimEmployerIndustryField(3, 4);
        await _accessObject.SeedEmployerIndustryFieldAsync(new[] { seed });

        var fetched = await _dimEmployerIndustryFieldService.GetEmployerIndustryFieldAsync(3, 4);
        Assert.Equal(3, fetched.EmployerId);
        Assert.Equal(4, fetched.IndustryFieldId);
    }
    
    [Fact]
    public async Task GetEmployerIndustryFieldAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployerIndustryFieldService.GetEmployerIndustryFieldAsync(99, 99));
    }
    
    [Fact]
    public async Task GetIndustryFieldsByEmployerIdAsync_Seeded_ReturnsOnlyThatEmployer()
    {
        var seeds = new[]
        {
            new DimEmployerIndustryField(5, 100),
            new DimEmployerIndustryField(5, 101),
            new DimEmployerIndustryField(6, 102),
        };
        await _accessObject.SeedEmployerIndustryFieldAsync(seeds);

        var list = (await _dimEmployerIndustryFieldService.GetIndustryFieldsByEmployerIdAsync(5)).ToList();
        Assert.Equal(2, list.Count);
        Assert.All(list, x => Assert.Equal(5, x.EmployerId));
        Assert.Contains(list, x => x.IndustryFieldId == 100);
        Assert.Contains(list, x => x.IndustryFieldId == 101);
    }
    
    [Fact]
    public async Task GetEmployersByIndustryFieldIdAsync_Seeded_ReturnsOnlyThatIndustry()
    {
        var seeds = new[]
        {
            new DimEmployerIndustryField(7, 200),
            new DimEmployerIndustryField(8, 200),
            new DimEmployerIndustryField(9, 201),
        };
        await _accessObject.SeedEmployerIndustryFieldAsync(seeds);

        var list = (await _dimEmployerIndustryFieldService.GetEmployersByIndustryFieldIdAsync(200)).ToList();
        Assert.Equal(2, list.Count);
        Assert.All(list, x => Assert.Equal(200, x.IndustryFieldId));
        Assert.Contains(list, x => x.EmployerId == 7);
        Assert.Contains(list, x => x.EmployerId == 8);
    }
    
    [Fact]
    public async Task DeleteEmployerIndustryFieldAsync_Existing_RemovesLink()
    {
        var seed = new DimEmployerIndustryField(11, 22);
        await _accessObject.SeedEmployerIndustryFieldAsync(new[] { seed });

        await _dimEmployerIndustryFieldService.DeleteEmployerIndustryFieldAsync(11, 22);

        var all = await _dimEmployerIndustryFieldService.GetAllEmployerIndustryFieldsAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task DeleteEmployerIndustryFieldAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _dimEmployerIndustryFieldService.DeleteEmployerIndustryFieldAsync(123, 456));
    }
}