using System.Linq;
using System.Threading.Tasks;
using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimEmployerServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimEmployerService _dimEmployerService;

    public DimEmployerServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimEmployerService = new DimEmployerService(_accessObject.EmployerRepository, NullLogger<DimEmployerService>.Instance);
    }
    public void Dispose() => _accessObject.Dispose();
    
    [Fact]
    public async Task GetAllEmployers_Empty_ReturnsEmpty()
    {
        var all = await _dimEmployerService.GetAllEmployersAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task GetAllEmployers_Seeded_ReturnsSeeded()
    {
        var seed = new List<DimEmployer>
        {
            new DimEmployer(1, "A Corp", "Tech", true),
            new DimEmployer(2, "B LLC", "Retail", false)
        };
        await _accessObject.SeedEmployerAsync(seed);
        var all = (await _dimEmployerService.GetAllEmployersAsync()).ToList();
        Assert.Contains(all, e => e.EmployerName == "A Corp" && e.Industry == "Tech");
        Assert.Contains(all, e => e.EmployerName == "B LLC"  && e.Industry == "Retail");
    }
}