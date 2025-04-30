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
    public async Task GetAllEmployerIndustryFields_Empty_ReturnsEmpty()
    {
        var all = await _dimEmployerIndustryFieldService.GetAllEmployerIndustryFieldsAsync();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task GetAllEmployerIndustryFields_Seeded_ReturnsSeeded()
    {
        var seed = new List<DimEmployerIndustryField>
        {
            new DimEmployerIndustryField(1, 100),
            new DimEmployerIndustryField(2, 200)
        };

        await _accessObject.SeedEmployerIndustryFieldAsync(seed);

        var all = (await _dimEmployerIndustryFieldService.GetAllEmployerIndustryFieldsAsync()).ToList();

        Assert.Contains(all, ei =>
            ei.EmployerId  == 1 &&
            ei.IndustryFieldId == 100
        );

        Assert.Contains(all, ee =>
            ee.EmployerId == 2 &&
            ee.IndustryFieldId == 200
        );
    }
}