using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Services.Dimensions.DimEducationService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimEducationServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimEducationService _dimEducationService;

    public DimEducationServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimEducationService = new DimEducationService(_accessObject.DimEducationRepository,
            NullLogger<DimEducationService>.Instance);
    }
    
    public void Dispose() => _accessObject.Dispose();
    
    [Fact]
    public async Task GetAllEducations_Empty_ReturnsEmpty()
    {
        var all = await _dimEducationService.GetAllEducationsAsync();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task GetAllEducations_Seeded_ReturnsSeeded()
    {
        var seed = new List<DimEducation>
        {
            new DimEducation(1, "Software Engineer", "01.01.01", 1,   1),
            new DimEducation(2, "Surgeon", "02.02.02", 2, 2)
        };

        await _accessObject.SeedEducationAsync(seed);

        var all = (await _dimEducationService.GetAllEducationsAsync()).ToList();
        Assert.Contains(all, d => d.Specialty == "Software Engineer");
        Assert.Contains(all, d => d.Specialty == "Surgeon");
    }
}