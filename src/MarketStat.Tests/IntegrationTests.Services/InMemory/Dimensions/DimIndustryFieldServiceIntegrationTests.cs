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
    public async Task GetAllEmployers_Empty_ReturnsEmpty()
    {
        var all = await _dimIndustryFieldService.GetAllIndustryFieldsAsync();
        Assert.Empty(all);
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
}