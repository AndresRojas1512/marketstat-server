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
    public async Task GetAllEducationLevels_Seeded()
    {
        var seed = new List<DimEducationLevel>
        {
            new DimEducationLevel(1, "Bachelors"),
            new DimEducationLevel(2, "Masters")
        };

        await _accessObject.SeedEducationLevelAsync(seed);
        var all = (await _dimEducationLevelService.GetAllEducationLevelsAsync()).ToList();
        Assert.Contains(all, e => e.EducationLevelName == "Bachelors");
        Assert.Contains(all, e => e.EducationLevelName == "Masters");
    }
}