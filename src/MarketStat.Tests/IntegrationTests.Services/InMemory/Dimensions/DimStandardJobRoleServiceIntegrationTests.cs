using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Services.Dimensions.DimStandardJobRoleService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimStandardJobRoleServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimStandardJobRoleService _dimStandardJobRoleService;

    public DimStandardJobRoleServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimStandardJobRoleService = new DimStandardJobRoleService(_accessObject.DimStandardJobRoleRepository,
            NullLogger<DimStandardJobRoleService>.Instance);
    }
    public void Dispose() => _accessObject.Dispose();
    
    [Fact]
    public async Task GetAllStandardJobRoles_Empty_ReturnsEmpty()
    {
        var all = await _dimStandardJobRoleService.GetAllStandardJobRolesAsync();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task GetAllStandardJobRoles_Seeded_ReturnsSeeded()
    {
        var seed = new List<DimStandardJobRole>
        {
            new DimStandardJobRole(1, "Software Engineer", 1),
            new DimStandardJobRole(2, "Mechanical Engineer", 2)
        };

        await _accessObject.SeedStandardJobRoleAsync(seed);

        var all = (await _dimStandardJobRoleService.GetAllStandardJobRolesAsync()).ToList();
        Assert.Contains(all, j => j.StandardJobRoleTitle == "Software Engineer");
        Assert.Contains(all, j => j.StandardJobRoleTitle == "Mechanical Engineer");
    }
}