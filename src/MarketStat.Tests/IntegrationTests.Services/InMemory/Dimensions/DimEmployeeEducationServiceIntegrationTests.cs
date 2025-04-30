using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployeeEducationService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimEmployeeEducationServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimEmployeeEducationService _dimEmployeeEducationService;

    public DimEmployeeEducationServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimEmployeeEducationService = new DimEmployeeEducationService(_accessObject.DimEmployeeEducationRepository,
            NullLogger<DimEmployeeEducationService>.Instance);
    }
    public void Dispose() => _accessObject.Dispose();
    
    [Fact]
    public async Task GetAllEmployeeEducations_Empty_ReturnsEmpty()
    {
        var all = await _dimEmployeeEducationService.GetAllEmployeeEducationsAsync();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task GetAllEmployeeEducations_Seeded_ReturnsSeeded()
    {
        var seed = new List<DimEmployeeEducation>
        {
            new DimEmployeeEducation(1, 100, 2010),
            new DimEmployeeEducation(2, 200, 2015)
        };

        await _accessObject.SeedEmployeeEducationsAsync(seed);

        var all = (await _dimEmployeeEducationService.GetAllEmployeeEducationsAsync()).ToList();

        Assert.Contains(all, ee =>
            ee.EmployeeId == 1 &&
            ee.EducationId == 100 &&
            ee.GraduationYear == 2010
        );

        Assert.Contains(all, ee =>
            ee.EmployeeId == 2 &&
            ee.EducationId == 200 &&
            ee.GraduationYear == 2015
        );
    }
}