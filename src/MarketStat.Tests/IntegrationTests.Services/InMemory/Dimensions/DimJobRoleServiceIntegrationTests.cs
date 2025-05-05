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
    public async Task CreateJobRoleAsync_ValidParameters_CreatesAndReturnsNewRole()
    {
        var role = await _dimJobRoleService.CreateJobRoleAsync("Developer", standardJobRoleId: 1, hierarchyLevelId: 2);

        Assert.True(role.JobRoleId > 0);
        Assert.Equal("Developer", role.JobRoleTitle);
        Assert.Equal(1, role.StandardJobRoleId);
        Assert.Equal(2, role.HierarchyLevelId);

        var all = (await _dimJobRoleService.GetAllJobRolesAsync()).ToList();
        Assert.Single(all);
        Assert.Equal(role.JobRoleId, all[0].JobRoleId);
    }
    
    [Fact]
    public async Task CreateJobRoleAsync_InvalidParameters_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimJobRoleService.CreateJobRoleAsync("", 1, 2));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimJobRoleService.CreateJobRoleAsync("X", 0, 2));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimJobRoleService.CreateJobRoleAsync("X", 1, 0));
    }
    
    [Fact]
    public async Task CreateJobRoleAsync_Duplicate_ThrowsException()
    {
        await _dimJobRoleService.CreateJobRoleAsync("Tester", 5, 6);
        await Assert.ThrowsAsync<Exception>(() =>
            _dimJobRoleService.CreateJobRoleAsync("Tester", 5, 6));
    }
    
    [Fact]
    public async Task GetJobRoleByIdAsync_Existing_ReturnsRole()
    {
        var seed = new DimJobRole(42, "Architect", 7, 8);
        await _accessObject.SeedJobRoleAsync(new[] { seed });

        var fetched = await _dimJobRoleService.GetJobRoleByIdAsync(42);
        Assert.Equal(42, fetched.JobRoleId);
        Assert.Equal("Architect", fetched.JobRoleTitle);
        Assert.Equal(7, fetched.StandardJobRoleId);
        Assert.Equal(8, fetched.HierarchyLevelId);
    }

    [Fact]
    public async Task GetJobRoleByIdAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _dimJobRoleService.GetJobRoleByIdAsync(999));
    }

    [Fact]
    public async Task GetAllJobRolesAsync_Seeded_ReturnsAll()
    {
        var seeds = new[]
        {
            new DimJobRole(1, "A", 1, 1),
            new DimJobRole(2, "B", 2, 2)
        };
        await _accessObject.SeedJobRoleAsync(seeds);

        var list = (await _dimJobRoleService.GetAllJobRolesAsync()).ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, r => r.JobRoleId == 1);
        Assert.Contains(list, r => r.JobRoleId == 2);
    }

    [Fact]
    public async Task UpdateJobRoleAsync_Existing_UpdatesAndReturns()
    {
        var original = new DimJobRole(5, "OldTitle", standardJobRoleId: 3, hierarchyLevelId: 4);
        await _accessObject.SeedJobRoleAsync(new[] { original });

        var updated = await _dimJobRoleService.UpdateJobRoleAsync(5, "NewTitle", 8, 9);
        Assert.Equal(5, updated.JobRoleId);
        Assert.Equal("NewTitle", updated.JobRoleTitle);
        Assert.Equal(8, updated.StandardJobRoleId);
        Assert.Equal(9, updated.HierarchyLevelId);

        var fetched = await _dimJobRoleService.GetJobRoleByIdAsync(5);
        Assert.Equal("NewTitle", fetched.JobRoleTitle);
    }

    [Fact]
    public async Task UpdateJobRoleAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _dimJobRoleService.UpdateJobRoleAsync(123, "X", 1, 1));
    }

    [Fact]
    public async Task UpdateJobRoleAsync_InvalidParameters_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimJobRoleService.UpdateJobRoleAsync(0, "X", 1, 1));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimJobRoleService.UpdateJobRoleAsync(1, "", 1, 1));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimJobRoleService.UpdateJobRoleAsync(1, "X", 0, 1));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimJobRoleService.UpdateJobRoleAsync(1, "X", 1, 0));
    }

    [Fact]
    public async Task DeleteJobRoleAsync_Existing_RemovesRole()
    {
        var seed = new DimJobRole(7, "Temp", 2, 3);
        await _accessObject.SeedJobRoleAsync(new[] { seed });

        await _dimJobRoleService.DeleteJobRoleAsync(7);
        var all = (await _dimJobRoleService.GetAllJobRolesAsync()).ToList();
        Assert.Empty(all);
    }

    [Fact]
    public async Task DeleteJobRoleAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _dimJobRoleService.DeleteJobRoleAsync(888));
    }
}