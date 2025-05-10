using System.Linq;
using System.Threading.Tasks;
using IntegrationTests.Services.AccessObject;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
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
    public async Task CreateEmployer_PersistsAndGeneratesId()
    {
        var created = await _dimEmployerService.CreateEmployerAsync("Acme Corp", true);
        Assert.True(created.EmployerId > 0);
        Assert.Equal("Acme Corp", created.EmployerName);
        Assert.True(created.IsPublic);

        var fetched = await _dimEmployerService.GetEmployerByIdAsync(created.EmployerId);
        Assert.Equal(created.EmployerId, fetched.EmployerId);
        Assert.Equal("Acme Corp",      fetched.EmployerName);
        Assert.True(fetched.IsPublic);
    }
    
    [Fact]
    public async Task GetEmployerById_Nonexistent_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployerService.GetEmployerByIdAsync(9999)
        );
    }
    
    [Fact]
    public async Task GetAllEmployers_Seeded_ReturnsSeeded()
    {
        await _accessObject.SeedEmployerAsync(new[]
        {
            new DimEmployer(1, "Foo Inc",  false),
            new DimEmployer(2, "Bar LLC",  true )
        });

        var all = (await _dimEmployerService.GetAllEmployersAsync()).ToList();
        Assert.Equal(2, all.Count);
        Assert.Contains(all, e => e.EmployerId == 1 && e.EmployerName == "Foo Inc");
        Assert.Contains(all, e => e.EmployerId == 2 && e.EmployerName == "Bar LLC");
    }
    
    [Fact]
    public async Task UpdateEmployer_PersistsChanges()
    {
        var created = await _dimEmployerService.CreateEmployerAsync("Old Name", false);

        var updated = await _dimEmployerService.UpdateEmployerAsync(
            created.EmployerId,
            "New Name",
            true
        );

        Assert.Equal(created.EmployerId, updated.EmployerId);
        Assert.Equal("New Name",        updated.EmployerName);
        Assert.True(updated.IsPublic);

        var fetched = await _dimEmployerService.GetEmployerByIdAsync(created.EmployerId);
        Assert.Equal("New Name", fetched.EmployerName);
        Assert.True(fetched.IsPublic);
    }
    
    [Fact]
    public async Task UpdateEmployer_InvalidId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _dimEmployerService.UpdateEmployerAsync(0, "Name", true)
        );
    }
    
    [Fact]
    public async Task UpdateEmployer_NotFound_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployerService.UpdateEmployerAsync(9999, "Name", false)
        );
    }

    [Fact]
    public async Task DeleteEmployer_RemovesIt()
    {
        var created = await _dimEmployerService.CreateEmployerAsync("ToDelete", false);
        await _dimEmployerService.DeleteEmployerAsync(created.EmployerId);
        
    }

    [Fact]
    public async Task DeleteEmployer_NotFound_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _dimEmployerService.DeleteEmployerAsync(9999)
        );
    }
}