using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Services.Dimensions.DimJobRoleService;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IntegrationTests.Services.InMemory.Dimensions;

public class DimJobRoleServiceIntegrationTests : IDisposable
{
    private readonly MarketStatAccessObjectInMemory _accessObject;
    private readonly IDimJobRoleService _dimJobRoleService;

    public DimJobRoleServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimJobRoleService = new DimJobRoleService(_accessObject.JobRoleRepository, NullLogger<DimJobRoleService>.Instance);
    }
    
    public void Dispose() => _accessObject.Dispose();
    
    [Fact]
    public async Task GetAllJobRoles_Empty_ReturnsEmpty()
    {
        var all = await _dimJobRoleService.GetAllJobRolesAsync();
        Assert.Empty(all);
    }
    
    [Fact]
    public async Task GetAllIndustryFields_Seeded_ReturnsSeeded()
    {
        var seed = new List<DimJobRole>
        {
            new DimJobRole(1, "Software Engineer", 1, 1),
            new DimJobRole(2, "Surgeon", 2, 2)
        };
        await _accessObject.SeedJobRoleAsync(seed);
        var all = (await _dimJobRoleService.GetAllJobRolesAsync()).ToList();
        Assert.Contains(all, i => i.JobRoleTitle == "Software Engineer");
        Assert.Contains(all, i => i.JobRoleTitle == "Surgeon");
    }
}