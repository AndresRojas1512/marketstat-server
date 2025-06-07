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
    
    private DimStandardJobRole CreateTestStandardJobRole(int id, string code, string title, int industryId)
    {
        return new DimStandardJobRole(id, code, title, industryId);
    }

    public DimStandardJobRoleServiceIntegrationTests()
    {
        _accessObject = new MarketStatAccessObjectInMemory();
        _dimStandardJobRoleService = _accessObject.DimStandardJobRoleService;
    }

    public void Dispose() => _accessObject.Dispose();

    [Fact]
    public async Task CreateStandardJobRoleAsync_ValidParameters_CreatesAndReturnsRole()
    {
        var code = "DEV";
        var title = "Developer";
        var fieldId = 7;
        
        var role = await _dimStandardJobRoleService.CreateStandardJobRoleAsync(code, title, fieldId);
        
        Assert.True(role.StandardJobRoleId > 0);
        Assert.Equal(code, role.StandardJobRoleCode);
        Assert.Equal(title, role.StandardJobRoleTitle);
        Assert.Equal(fieldId, role.IndustryFieldId);

        var all = (await _dimStandardJobRoleService.GetAllStandardJobRolesAsync()).ToList();
        Assert.Single(all);
        Assert.Equal(role.StandardJobRoleId, all[0].StandardJobRoleId);
        Assert.Equal(code, all[0].StandardJobRoleCode);
    }

    [Fact]
    public async Task GetStandardJobRoleByIdAsync_Existing_ReturnsRole()
    {
        var seed = CreateTestStandardJobRole(42, "ANL-42", "Analyst", 3);
        await _accessObject.SeedStandardJobRoleAsync(new[] { seed });
        
        var fetched = await _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(42);
        
        Assert.NotNull(fetched);
        Assert.Equal(seed.StandardJobRoleId, fetched.StandardJobRoleId);
        Assert.Equal(seed.StandardJobRoleCode, fetched.StandardJobRoleCode);
        Assert.Equal(seed.StandardJobRoleTitle, fetched.StandardJobRoleTitle);
        Assert.Equal(seed.IndustryFieldId, fetched.IndustryFieldId);
    }

    [Fact]
    public async Task GetStandardJobRoleByIdAsync_NotFound_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(999));
    }

    [Fact]
    public async Task GetAllStandardJobRolesAsync_Seeded_ReturnsAll()
    {
        var seeds = new[]
        {
            CreateTestStandardJobRole(1, "QA", "QA Engineer", 1),
            CreateTestStandardJobRole(2, "DEVOPS", "DevOps Engineer", 2),
        };
        await _accessObject.SeedStandardJobRoleAsync(seeds);
        
        var list = (await _dimStandardJobRoleService.GetAllStandardJobRolesAsync()).ToList();
        
        Assert.Equal(2, list.Count);
        Assert.Contains(list, r => r.StandardJobRoleCode == "QA" && r.IndustryFieldId == 1);
        Assert.Contains(list, r => r.StandardJobRoleCode == "DEVOPS" && r.IndustryFieldId == 2);
    }

    [Fact]
    public async Task UpdateStandardJobRoleAsync_Existing_UpdatesAndReturns()
    {
        var original = CreateTestStandardJobRole(5, "OLD-TTL", "OldTitle", 4);
        await _accessObject.SeedStandardJobRoleAsync(new[] { original });
        
        var newCode = "NEW-TTL";
        var newTitle = "NewTitle";
        var newIndustryId = 9;
        
        var updated = await _dimStandardJobRoleService.UpdateStandardJobRoleAsync(5, newCode, newTitle, newIndustryId);
        
        Assert.Equal(5, updated.StandardJobRoleId);
        Assert.Equal(newCode, updated.StandardJobRoleCode);
        Assert.Equal(newTitle, updated.StandardJobRoleTitle);
        Assert.Equal(newIndustryId, updated.IndustryFieldId);

        var fetched = await _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(5);
        Assert.NotNull(fetched);
        Assert.Equal(newCode, fetched.StandardJobRoleCode);
        Assert.Equal(newTitle, fetched.StandardJobRoleTitle);
    }

    [Fact]
    public async Task UpdateStandardJobRoleAsync_NotFound_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimStandardJobRoleService.UpdateStandardJobRoleAsync(123, "CODE", "Title", 1));
    }

    [Fact]
    public async Task DeleteStandardJobRoleAsync_Existing_RemovesRole()
    {
        var seed = CreateTestStandardJobRole(7, "DEL-7", "ToDelete", 8);
        await _accessObject.SeedStandardJobRoleAsync(new[] { seed });

        await _dimStandardJobRoleService.DeleteStandardJobRoleAsync(7);

        await Assert.ThrowsAsync<NotFoundException>(() => 
            _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(7)
        );
    }

    [Fact]
    public async Task DeleteStandardJobRoleAsync_NotFound_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimStandardJobRoleService.DeleteStandardJobRoleAsync(999));
    }
}