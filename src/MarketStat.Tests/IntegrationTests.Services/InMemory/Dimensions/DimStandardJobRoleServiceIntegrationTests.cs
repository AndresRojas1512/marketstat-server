using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
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
    public async Task CreateStandardJobRoleAsync_ValidParameters_CreatesAndReturnsRole()
    {
        var title = "Developer";
        var fieldId = 7;

        var role = await _dimStandardJobRoleService.CreateStandardJobRoleAsync(title, fieldId);

        Assert.True(role.StandardJobRoleId > 0);
        Assert.Equal(title, role.StandardJobRoleTitle);
        Assert.Equal(fieldId, role.IndustryFieldId);

        var all = (await _dimStandardJobRoleService.GetAllStandardJobRolesAsync()).ToList();
        Assert.Single(all);
        Assert.Equal(role.StandardJobRoleId, all[0].StandardJobRoleId);
    }

    [Fact]
    public async Task GetStandardJobRoleByIdAsync_Existing_ReturnsRole()
    {
        var seed = new DimStandardJobRole(42, "Analyst", 3);
        await _accessObject.SeedStandardJobRoleAsync(new[] { seed });

        var fetched = await _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(42);

        Assert.Equal(seed.StandardJobRoleId, fetched.StandardJobRoleId);
        Assert.Equal(seed.StandardJobRoleTitle, fetched.StandardJobRoleTitle);
        Assert.Equal(seed.IndustryFieldId, fetched.IndustryFieldId);
    }

    [Fact]
    public async Task GetStandardJobRoleByIdAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(999));
    }

    [Fact]
    public async Task GetAllStandardJobRolesAsync_Seeded_ReturnsAll()
    {
        var seeds = new[]
        {
            new DimStandardJobRole(1, "A", 1),
            new DimStandardJobRole(2, "B", 2),
        };
        await _accessObject.SeedStandardJobRoleAsync(seeds);

        var list = (await _dimStandardJobRoleService.GetAllStandardJobRolesAsync()).ToList();

        Assert.Equal(2, list.Count);
        Assert.Contains(list, r => r.StandardJobRoleTitle == "A" && r.IndustryFieldId == 1);
        Assert.Contains(list, r => r.StandardJobRoleTitle == "B" && r.IndustryFieldId == 2);
    }

    [Fact]
    public async Task UpdateStandardJobRoleAsync_Existing_UpdatesAndReturns()
    {
        var original = new DimStandardJobRole(5, "OldTitle", 4);
        await _accessObject.SeedStandardJobRoleAsync(new[] { original });

        var updated = await _dimStandardJobRoleService.UpdateStandardJobRoleAsync(5, "NewTitle", 9);

        Assert.Equal(5, updated.StandardJobRoleId);
        Assert.Equal("NewTitle", updated.StandardJobRoleTitle);
        Assert.Equal(9, updated.IndustryFieldId);

        var fetched = await _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(5);
        Assert.Equal("NewTitle", fetched.StandardJobRoleTitle);
    }

    [Fact]
    public async Task UpdateStandardJobRoleAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimStandardJobRoleService.UpdateStandardJobRoleAsync(123, "X", 1));
    }

    [Fact]
    public async Task DeleteStandardJobRoleAsync_Existing_RemovesRole()
    {
        var seed = new DimStandardJobRole(7, "ToDelete", 8);
        await _accessObject.SeedStandardJobRoleAsync(new[] { seed });

        await _dimStandardJobRoleService.DeleteStandardJobRoleAsync(7);

        var all = (await _dimStandardJobRoleService.GetAllStandardJobRolesAsync()).ToList();
        Assert.Empty(all);
    }

    [Fact]
    public async Task DeleteStandardJobRoleAsync_NotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimStandardJobRoleService.DeleteStandardJobRoleAsync(999));
    }
}