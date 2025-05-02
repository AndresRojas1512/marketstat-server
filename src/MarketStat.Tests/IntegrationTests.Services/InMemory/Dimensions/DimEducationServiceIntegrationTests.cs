using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Services.Dimensions.DimEducationService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimEducationServiceIntegrationTests
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimEducationService _dimEducationService;

    public DimEducationServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimEducationService = new DimEducationService(_accessObject.DimEducationRepository,
            NullLogger<DimEducationService>.Instance);
    }
    
    [Fact]
    public async Task GetAllEducations_Empty_ReturnsEmpty()
    {
        var all = await _dimEducationService.GetAllEducationsAsync();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task GetAllEducation_Seeded_ReturnsSeeded()
    {
        var seed = new List<DimEducation>
        {
            new DimEducation(1, "Software Engineer",  1,   1),
            new DimEducation(2, "Surgeon", 2, 2)
        };

        foreach (var d in seed)
            await _accessObject.DimEducationRepository.AddEducationAsync(d);

        var all = (await _dimEducationService.GetAllEducationsAsync()).ToList();
        Assert.Contains(all, d => d.Specialization == "Software Engineer");
        Assert.Contains(all, d => d.Specialization == "Surgeon");
    }
}