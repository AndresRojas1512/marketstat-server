using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Services.Dimensions.DimEmployeeService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimEmployeeServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimEmployeeService _dimEmployeeService;

    public DimEmployeeServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimEmployeeService =
            new DimEmployeeService(_accessObject.DimEmployeeRepository, NullLogger<DimEmployeeService>.Instance);
    }
    
    public void Dispose() => _accessObject.Dispose();
    
    [Fact]
    public async Task GetAllEmployees_Empty_ReturnsEmpty()
    {
        var all = await _dimEmployeeService.GetAllEmployeesAsync();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task GetAllEmployees_Seeded_ReturnsSeeded()
    {
        var seed = new List<DimEmployee>
        {
            new DimEmployee(1, new DateOnly(1980, 1,  1), new DateOnly(2000, 1,  1)),
            new DimEmployee(2, new DateOnly(1981, 4, 15), new DateOnly(2000, 1, 1))
        };

        await _accessObject.SeedEmployeeAsync(seed);

        var all = (await _dimEmployeeService.GetAllEmployeesAsync()).ToList();
        
        Assert.Contains(all, e =>
            e.BirthDate       == new DateOnly(1980, 1,  1) &&
            e.CareerStartDate == new DateOnly(2000, 1,  1)
        );

        Assert.Contains(all, e =>
            e.BirthDate       == new DateOnly(1981, 4, 15) &&
            e.CareerStartDate == new DateOnly(2000, 1,  1)
        );
    }
}